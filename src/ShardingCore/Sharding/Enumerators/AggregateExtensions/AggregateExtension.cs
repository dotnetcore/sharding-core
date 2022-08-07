using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.Enumerators.AggregateExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Tuesday, 02 February 2021 14:44:36
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// https://stackoverflow.com/questions/17490080/how-to-do-a-sum-using-dynamic-linq
    /// </summary>
    internal static class AggregateExtension
    {
        public static TSource CopyTSource<TSource>(TSource source)
        {
            var anonType = source.GetType();
            var allProperties = anonType.GetProperties();
            var allPropertyTypes= allProperties.Select(o=>o.PropertyType).ToArray();
            var constantExpressions = allProperties.Select(o => Expression.Constant(o.GetValue(source))).ToArray();

            var exp = Expression.New(
                anonType.GetConstructor(allPropertyTypes),
                constantExpressions);
            var lambda = LambdaExpression.Lambda(exp);
            TSource myObj = (TSource)lambda.Compile().DynamicInvoke();
            return myObj;

        }
        [ExcludeFromCodeCoverage]
        public static Func<TSource, TSource> CreateNewStatement<TSource>(string fields)
        {
            // input parameter "o"
            var xParameter = Expression.Parameter(typeof(TSource), "o");

            // new statement "new Data()"

            // create initializers
            var bindings = fields.Split(',').Select(o => o.Trim())
                .Select(o => {

                        // property "Field1"
                        var mi = typeof(TSource).GetProperty(o);

                        // original value "o.Field1"
                        var xOriginal = Expression.Property(xParameter, mi);

                        // set value "Field1 = o.Field1"
                        return Expression.Bind(mi, xOriginal);
                    }
                );

            var xNew = Expression.New(typeof(TSource));
            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var xInit = Expression.MemberInit(xNew, bindings);

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var lambda = Expression.Lambda<Func<TSource, TSource>>(xInit, xParameter);

            // compile to Func<Data, Data>
            return lambda.Compile();
        }
        [ExcludeFromCodeCoverage]
        public static object Count(this IQueryable source, PropertyInfo property)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (property == null) throw new ArgumentNullException(nameof(property));
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");
            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);
            Expression selector = Expression.Lambda(getter, parameter);

            MethodInfo sumMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Sum)
                     && m.ReturnType == property.PropertyType
                     && m.IsGenericMethod);
            var genericSumMethod = sumMethod.MakeGenericMethod(new[] {source.ElementType});

            var callExpression = Expression.Call(
                null,
                genericSumMethod,
                new[] {source.Expression, Expression.Quote(selector)});

            return source.Provider.Execute(callExpression);
        }
        [ExcludeFromCodeCoverage]
        public static object Count(this IQueryable source, string propertyName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            PropertyInfo property = source.ElementType.GetProperty(propertyName);

            return source.Count(property);
        }

        private static MethodCallExpression CreateSumByProperty(this IQueryable source, PropertyInfo property)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (!property.PropertyType.IsNumericType())
                throw new ShardingCoreInvalidOperationException(
                    $"method sum cant calc type :[{property.PropertyType}]");
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");
            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);
            Expression selector = Expression.Lambda(getter, parameter);
            MethodInfo sumMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Sum)
                     && m.ReturnType == property.PropertyType
                     && m.IsGenericMethod);

            var genericSumMethod = sumMethod.MakeGenericMethod(new[] { source.ElementType });

            var callExpression = Expression.Call(
                null,
                genericSumMethod,
                new[] { source.Expression, Expression.Quote(selector) });
            return callExpression;
        }
        
        private static MethodCallExpression CreateSumByConstant<TSelect>(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var valueType = typeof(TSelect);
            if (!valueType.IsNumericType())
                throw new ShardingCoreInvalidOperationException(
                    $"method sum cant calc type :[{valueType}]");
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");
            // MemberExpression getter = Expression.MakeMemberAccess(parameter, property);
            Expression selector = Expression.Lambda(parameter);
            MethodInfo sumMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Sum)
                     && m.ReturnType == valueType
                     && m.IsGenericMethod);

            var genericSumMethod = sumMethod.MakeGenericMethod(new[] { source.ElementType });

            var callExpression = Expression.Call(
                null,
                genericSumMethod,
                new[] { source.Expression, Expression.Quote(selector) });
            return callExpression;
        }
        /// <summary>
        /// 根据属性求和
        /// </summary>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object SumByProperty(this IQueryable source, PropertyInfo property)
        {
            var callExpression = CreateSumByProperty(source, property);
            return source.Provider.Execute(callExpression);
        }
        public static TSelect SumByProperty<TSelect>(this IQueryable source, PropertyInfo property)
        {
            var callExpression = CreateSumByProperty(source, property);
            return source.Provider.Execute<TSelect>(callExpression);
        }
        public static TSelect SumByConstant<TSelect>(this IQueryable source)
        {
            var callExpression = CreateSumByConstant<TSelect>(source);
            return source.Provider.Execute<TSelect>(callExpression);
        }
        /// <summary>
        /// 根据属性求和
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object SumByPropertyName(this IQueryable source, string propertyName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            PropertyInfo property = source.ElementType.GetProperty(propertyName);
            return source.SumByProperty(property);
        }
        /// <summary>
        /// 对
        /// </summary>
        /// <typeparam name="TSelect"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TSelect SumByPropertyName<TSelect>(this IQueryable source, string propertyName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            PropertyInfo property = source.ElementType.GetProperty(propertyName);
            return source.SumByProperty<TSelect>(property);
        }
        //public static object Average(this IQueryable source, string member)
        //{
        //    if (source == null) throw new ArgumentNullException(nameof(source));
        //    if (member == null) throw new ArgumentNullException(nameof(member));

        //    // The most common variant of Queryable.Average() expects a lambda.
        //    // Since we just have a string to a property, we need to create a
        //    // lambda from the string in order to pass it to the sum method.

        //    // Lets create a ((TSource s) => s.Average ). First up, the parameter "s":
        //    ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");

        //    // Followed by accessing the Average property of "s" (s.Average):
        //    PropertyInfo property = source.ElementType.GetProperty(member);
        //    MemberExpression getter = Expression.MakeMemberAccess(parameter, property);

        //    // And finally, we create a lambda from that. First specifying on what
        //    // to execute when the lambda is called, and finally the parameters of the lambda.
        //    Expression selector = Expression.Lambda(getter, parameter);

        //    // There are a lot of Queryable.Average() overloads with different
        //    // return types  (double, int, decimal, double?, int?, etc...).
        //    // We're going to find one that matches the type of our property.
        //    MethodInfo avgMethod = typeof(Queryable).GetMethods().First(
        //        m => m.Name == nameof(Queryable.Average)
        //             && m.ReturnType == property.PropertyType
        //             && m.GetParameters().Length==2
        //             && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments()[1]==property.PropertyType
        //             && m.IsGenericMethod);

        //    // Now that we have the correct method, we need to know how to call the method.
        //    // Note that the Queryable.SumByProperty<TSource>(source, selector) has a generic type,
        //    // which we haven't resolved yet. Good thing is that we can use copy the one from
        //    // our initial source expression.
        //    var genericAvgMethod = avgMethod.MakeGenericMethod(new[] {source.ElementType});

        //    // TSource, source and selector are now all resolved. We now know how to call
        //    // the sum-method. We're not going to call it here, we just express how we're going
        //    // call it.
        //    var callExpression = Expression.Call(
        //        null,
        //        genericAvgMethod,
        //        new[] {source.Expression, Expression.Quote(selector)});

        //    // Pass it down to the query provider. This can be a simple LinqToObject-datasource,
        //    // but also a more complex datasource (such as LinqToSql). Anyway, it knows what to
        //    // do.
        //    return source.Provider.Execute(callExpression);
        //}
        [ExcludeFromCodeCoverage]
        public static object Max(this IQueryable source, string propertyName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            PropertyInfo property = source.ElementType.GetProperty(propertyName);

            return source.Max(property);
        }
        public static object Max(this IQueryable source, PropertyInfo property)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (property == null) throw new ArgumentNullException(nameof(property));

            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");

            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);

            LambdaExpression selector = Expression.Lambda(getter, parameter);

            MethodInfo maxMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Max)
                     && m.GetParameters().Length==2 && typeof(Expression).IsAssignableFrom(m.GetParameters()[1].ParameterType)
                     && m.IsGenericMethod);

            var genericMaxMethod = maxMethod.MakeGenericMethod(new[] {source.ElementType,selector.Body.Type});

            var callExpression = Expression.Call(
                null,
                genericMaxMethod,
                new[] {source.Expression, Expression.Quote(selector)});

            return source.Provider.Execute(callExpression);
        }
        [ExcludeFromCodeCoverage]
        public static object Min(this IQueryable source, string propertyName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            PropertyInfo property = source.ElementType.GetProperty(propertyName);
            return source.Min(property);
        }
        public static object Min(this IQueryable source, PropertyInfo property)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (property == null) throw new ArgumentNullException(nameof(property));

            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");

            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);

            LambdaExpression selector = Expression.Lambda(getter, parameter);

            
            MethodInfo minMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Min)
                     && m.GetParameters().Length==2 &&typeof(Expression).IsAssignableFrom(m.GetParameters()[1].ParameterType)
                     && m.IsGenericMethod);

            var genericMinMethod = minMethod.MakeGenericMethod(new[] {source.ElementType,selector.Body.Type});

            var callExpression = Expression.Call(
                null,
                genericMinMethod,
                new[] {source.Expression, Expression.Quote(selector)});

            return source.Provider.Execute(callExpression);
        }
        /// <summary>
        /// 获取平均数和 [{avg1,count1},{avg2,count2}....]=>sum(avg1...n*count1...n)/sum(count1...n)
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="averagePropertyName">聚合函数average属性名</param>
        /// <param name="countPropertyName">聚合函数count属性名</param>
        /// <param name="resultType">平均值返回结果:int/int=double</param>
        [ExcludeFromCodeCoverage]
        public static object AverageWithCount(this IQueryable source, string averagePropertyName, string countPropertyName, Type resultType)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (averagePropertyName == null) throw new ArgumentNullException(nameof(averagePropertyName));
            if (countPropertyName == null) throw new ArgumentNullException(nameof(countPropertyName));
            var averageProperty = source.ElementType.GetProperty(averagePropertyName);
            var countProperty = source.ElementType.GetProperty(countPropertyName);
            return source.AverageWithCount(averageProperty, countProperty, resultType);
        }
        public static object AverageWithCount(this IQueryable source, PropertyInfo averageProperty, PropertyInfo countProperty, Type resultType)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (averageProperty == null) throw new ArgumentNullException(nameof(averageProperty));
            if (countProperty == null) throw new ArgumentNullException(nameof(countProperty));
            //获取sum
            var sum = source.AverageSum(averageProperty, countProperty);
            var count = source.SumByProperty(countProperty);
            return AverageConstant(sum, count,resultType);
            //var constantSum = Expression.Constant(sum);
            //var constantCount = Expression.Constant(count);
            //var unaryExpression = Expression.Convert(constantCount, sum.GetType());
            //var binaryExpression = Expression.Divide(constantSum, unaryExpression);
            //var invoke = Expression.Lambda(binaryExpression).Compile().DynamicInvoke();
            //return invoke;
        }

        public static object AverageConstant(object sum, object count,Type resultType)
        {

            Expression constantSum = Expression.Constant(sum);
            //如果计算类型和返回类型不一致先转成一致
            if(sum.GetType()!=resultType)
                constantSum = Expression.Convert(constantSum, resultType);
            Expression constantCount = Expression.Constant(count);
            //如果计算类型和返回类型不一致先转成一致
            if (count.GetType() != resultType)
                constantCount = Expression.Convert(constantCount, resultType);
            var binaryExpression = Expression.Divide(constantSum, constantCount);
            var invoke = Expression.Lambda(binaryExpression).Compile().DynamicInvoke();
            return invoke;
        }
        public static TResult AverageConstant<TSum,TCount,TResult>(TSum sum, TCount count)
        {
            var resultType = typeof(TResult);
            Expression constantSum = Expression.Constant(sum);
            //如果计算类型和返回类型不一致先转成一致
            if (sum.GetType() != resultType)
                constantSum = Expression.Convert(constantSum, resultType);
            Expression constantCount = Expression.Constant(count);
            //如果计算类型和返回类型不一致先转成一致
            if (count.GetType() != resultType)
                constantCount = Expression.Convert(constantCount, resultType);
            var binaryExpression = Expression.Divide(constantSum, constantCount);
            var invoke = Expression.Lambda<Func<TResult>>(binaryExpression).Compile()();
            return invoke;
        }
        //private static readonly Type _divideFirstDecimalType=typeof(decimal);
        //private static readonly Type _divideSecondDoubleType=typeof(double);
        //private static readonly Type _divideThirdFloatType=typeof(float);
        //private static Type GetConvertPriorityType(Type sum, Type count, Type result)
        //{
        //    if (_divideFirstDecimalType == sum || _divideFirstDecimalType == count || _divideFirstDecimalType == result)
        //        return _divideFirstDecimalType;
        //    if (_divideSecondDoubleType == sum || _divideSecondDoubleType == count || _divideSecondDoubleType == result)
        //        return _divideSecondDoubleType;
        //    if (_divideThirdFloatType == sum || _divideThirdFloatType == count || _divideThirdFloatType == result)
        //        return _divideThirdFloatType;
        //    return result;
        //}

        /// <summary>
        /// 获取平均数和 [{avg1,sum1},{avg2,sum2}....]=>sum(sum1...n)/sum(sum1...n/avg1...n)
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="averagePropertyName">聚合函数average属性名</param>
        /// <param name="sumPropertyName">聚合函数sum属性名</param>
        /// <param name="resultType">平均值返回结果:int/int=double</param>
        [ExcludeFromCodeCoverage]
        public static object AverageWithSum(this IQueryable source, string averagePropertyName, string sumPropertyName, Type resultType)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (averagePropertyName == null) throw new ArgumentNullException(nameof(averagePropertyName));
            if (sumPropertyName == null) throw new ArgumentNullException(nameof(sumPropertyName));
            var averageProperty = source.ElementType.GetProperty(averagePropertyName);
            var sumProperty = source.ElementType.GetProperty(sumPropertyName);
            return source.AverageWithSum(averageProperty, sumProperty, resultType);
        }
        public static object AverageWithSum(this IQueryable source, PropertyInfo averageProperty, PropertyInfo sumProperty, Type resultType)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (averageProperty == null) throw new ArgumentNullException(nameof(averageProperty));
            if (sumProperty == null) throw new ArgumentNullException(nameof(sumProperty));
            var count = source.AverageCount(averageProperty, sumProperty);
            var sum = source.SumByProperty(sumProperty);
            return AverageConstant(sum, count, resultType);
        }
        /// <summary>
        /// 获取平均数和 [{avg1,count1},{avg2,count2}....]=>sum(avg1...n*count1...n)/sum(count1...n)
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="averageProperty">聚合函数average属性名</param>
        /// <param name="countProperty">聚合函数count属性名</param>
        /// <returns></returns>
        private static object AverageSum(this IQueryable source, PropertyInfo averageProperty, PropertyInfo countProperty)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (averageProperty == null) throw new ArgumentNullException(nameof(averageProperty));
            if (countProperty == null) throw new ArgumentNullException(nameof(countProperty));
            //o=>
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s"); 
            //o.avg
            MemberExpression averageMember = Expression.MakeMemberAccess(parameter, averageProperty);
            //o.count
            MemberExpression countMember = Expression.MakeMemberAccess(parameter, countProperty);
            //Convert(o.count,o.avg.GetType()) 必须要同类型才能计算
            var countConvertExpression = Expression.Convert(countMember, averageProperty.PropertyType);
            //o.avg*Convert(o.count,o.avg.GetType())
            var multiply = Expression.Multiply(averageMember, countConvertExpression);

            //o=>o.avg*Convert(o.count,o.avg.GetType())
            Expression selector = Expression.Lambda(multiply, parameter);
            MethodInfo sumMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Sum)
                     && m.ReturnType == averageProperty.PropertyType
                     && m.IsGenericMethod);

            var genericSumMethod = sumMethod.MakeGenericMethod(new[] { source.ElementType });

            var callExpression = Expression.Call(
                null,
                genericSumMethod,
                new[] { source.Expression, Expression.Quote(selector) });

            return source.Provider.Execute(callExpression);

        }
        /// <summary>
        /// 获取平均数个数 [{avg1,sum1},{avg2,sum2}....]=>sum(sum1..n)/sum(sum1...n/avg1...n)
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="averageProperty">聚合函数average属性名</param>
        /// <param name="sumProperty">聚合函数count属性名</param>
        /// <returns></returns>
        private static object AverageCount(this IQueryable source, PropertyInfo averageProperty, PropertyInfo sumProperty)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (averageProperty == null) throw new ArgumentNullException(nameof(averageProperty));
            if (sumProperty == null) throw new ArgumentNullException(nameof(sumProperty));
            //o=>
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s"); 
            //o.avg
            MemberExpression averageMember = Expression.MakeMemberAccess(parameter, averageProperty);
            //o.sum
            MemberExpression sumMember = Expression.MakeMemberAccess(parameter, sumProperty);
            //Convert(o.sum,o.avg.GetType()) 必须要同类型才能计算
            var sumConvertExpression = Expression.Convert(sumMember, averageProperty.PropertyType);
            //Convert(o.sum,o.avg.GetType())/o.avg
            var divide = Expression.Divide(sumConvertExpression, averageMember);

            //o=>Convert(o.sum,o.avg.GetType())/o.avg
            Expression selector = Expression.Lambda(divide, parameter);
            MethodInfo sumMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Sum)
                     && m.ReturnType == averageProperty.PropertyType
                     && m.IsGenericMethod);

            var genericSumMethod = sumMethod.MakeGenericMethod(new[] { source.ElementType });

            var callExpression = Expression.Call(
                null,
                genericSumMethod,
                new[] { source.Expression, Expression.Quote(selector) });

            return source.Provider.Execute(callExpression);

        }
    }
}