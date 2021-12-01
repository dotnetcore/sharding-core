using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Core
{
    internal class ShardingCreatorHelper
    {
        private static ConcurrentDictionary<Type, Func<object[], object>> _creator;
        private ShardingCreatorHelper() { }

        static ShardingCreatorHelper()
        {
            _creator = new ConcurrentDictionary<Type, Func<object[], object>>();
        }
        public static object CreateInstance(Type targetType,params object[] args)
        {
            var creator = _creator.GetOrAdd(targetType,key=> GetActivator(key));
            return creator(args);
        }

        private static Func<object[],object> GetActivator(Type targetType)
        {
            ConstructorInfo ctor = targetType.GetConstructors().First();
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            //compile it
            var compiled =
                Expression.Lambda<Func<object[],object>>(newExp, param).Compile();

            return compiled;
        }
    }
}
