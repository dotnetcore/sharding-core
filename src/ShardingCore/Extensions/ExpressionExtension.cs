using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Exceptions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 November 2020 22:06:21
    * @Email: 326308290@qq.com
    */
    public static class ExpressionExtension
    {
        public static void SetPropertyValue<T>(this T t, string name, object value)
        {
            Type type = t.GetType();
            PropertyInfo p = type.GetProperty(name);
            if (p == null)
            {
                throw new Exception($"type:{typeof(T)} not found [{name}] properity ");
            }

            var param_obj = Expression.Parameter(type);
            var param_val = Expression.Parameter(typeof(object));
            var body_obj = Expression.Convert(param_obj, type);
            var body_val = Expression.Convert(param_val, p.PropertyType);

            //获取设置属性的值的方法
            var setMethod = p.GetSetMethod(true);

            //如果只是只读,则setMethod==null
            if (setMethod != null)
            {
                var body = Expression.Call(param_obj, p.GetSetMethod(), body_val);
                var setValue = Expression.Lambda<Action<T, object>>(body, param_obj, param_val).Compile();
                setValue(t, value);
            }
            else
            {
                t.GetType().GetFields( BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o=>o.Name==$"<{name}>i__Field")
                  .SetValue(t,value);
            }
        }
        public static object GetValueByExpression(this object obj, string propertyExpression)
        {
            var entityType = obj.GetType();
            PropertyInfo property;
            //Expression propertyAccess;
            //var parameter = Expression.Parameter(entityType, "o");

            if (propertyExpression.Contains("."))
            {
                String[] childProperties = propertyExpression.Split('.');
                property = entityType.GetProperty(childProperties[0]);
                //propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (int i = 1; i < childProperties.Length; i++)
                {
                    if (property == null)
                    {
                        throw new ShardingCoreException($"property:[{propertyExpression}] not in type:[{entityType}]");
                    }
                    property = property.PropertyType.GetProperty(childProperties[i]);
                    //propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = entityType.GetProperty(propertyExpression);
                //propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }

            if (property == null)
            {
                throw new ShardingCoreException($"property:[{propertyExpression}] not in type:[{entityType}]");
            }

            return property.GetValue(obj);
            //var lambda = Expression.Lambda(propertyAccess, parameter);
            //Delegate fn = lambda.Compile();
            //return fn.DynamicInvoke(obj);
        }

        /// <summary>
        /// 添加And条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.AndAlso(second, Expression.AndAlso);
        }

        /// <summary>
        /// 添加Or条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.AndAlso(second, Expression.OrElse);
        }

        /// <summary>
        /// 合并表达式以及参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr1"></param>
        /// <param name="expr2"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2,
            Func<Expression, Expression, BinaryExpression> func)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);
            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(
                func(left, right), parameter);
        }


        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}