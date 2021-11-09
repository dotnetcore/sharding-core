using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        
        public static object Count(this IQueryable source, string member)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (member == null) throw new ArgumentNullException(nameof(member));

            // The most common variant of Queryable.Sum() expects a lambda.
            // Since we just have a string to a property, we need to create a
            // lambda from the string in order to pass it to the sum method.

            // Lets create a ((TSource s) => s.Sum ). First up, the parameter "s":
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");

            // Followed by accessing the Price property of "s" (s.Sum):
            PropertyInfo property = source.ElementType.GetProperty(member);
            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);

            // And finally, we create a lambda from that. First specifying on what
            // to execute when the lambda is called, and finally the parameters of the lambda.
            Expression selector = Expression.Lambda(getter, parameter);

            // There are a lot of Queryable.Sum() overloads with different
            // return types  (double, int, decimal, double?, int?, etc...).
            // We're going to find one that matches the type of our property.
            MethodInfo sumMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Sum)
                     && m.ReturnType == property.PropertyType
                     && m.IsGenericMethod);

            // Now that we have the correct method, we need to know how to call the method.
            // Note that the Queryable.Sum<TSource>(source, selector) has a generic type,
            // which we haven't resolved yet. Good thing is that we can use copy the one from
            // our initial source expression.
            var genericSumMethod = sumMethod.MakeGenericMethod(new[] {source.ElementType});

            // TSource, source and selector are now all resolved. We now know how to call
            // the sum-method. We're not going to call it here, we just express how we're going
            // call it.
            var callExpression = Expression.Call(
                null,
                genericSumMethod,
                new[] {source.Expression, Expression.Quote(selector)});

            // Pass it down to the query provider. This can be a simple LinqToObject-datasource,
            // but also a more complex datasource (such as LinqToSql). Anyway, it knows what to
            // do.
            return source.Provider.Execute(callExpression);
        }
        public static object Sum(this IQueryable source, string member)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (member == null) throw new ArgumentNullException(nameof(member));

            // The most common variant of Queryable.Sum() expects a lambda.
            // Since we just have a string to a property, we need to create a
            // lambda from the string in order to pass it to the sum method.

            // Lets create a ((TSource s) => s.Sum ). First up, the parameter "s":
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");

            // Followed by accessing the Price property of "s" (s.Sum):
            PropertyInfo property = source.ElementType.GetProperty(member);
            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);

            // And finally, we create a lambda from that. First specifying on what
            // to execute when the lambda is called, and finally the parameters of the lambda.
            Expression selector = Expression.Lambda(getter, parameter);

            // There are a lot of Queryable.Sum() overloads with different
            // return types  (double, int, decimal, double?, int?, etc...).
            // We're going to find one that matches the type of our property.
            MethodInfo sumMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Sum)
                     && m.ReturnType == property.PropertyType
                     && m.IsGenericMethod);

            // Now that we have the correct method, we need to know how to call the method.
            // Note that the Queryable.Sum<TSource>(source, selector) has a generic type,
            // which we haven't resolved yet. Good thing is that we can use copy the one from
            // our initial source expression.
            var genericSumMethod = sumMethod.MakeGenericMethod(new[] {source.ElementType});

            // TSource, source and selector are now all resolved. We now know how to call
            // the sum-method. We're not going to call it here, we just express how we're going
            // call it.
            var callExpression = Expression.Call(
                null,
                genericSumMethod,
                new[] {source.Expression, Expression.Quote(selector)});

            // Pass it down to the query provider. This can be a simple LinqToObject-datasource,
            // but also a more complex datasource (such as LinqToSql). Anyway, it knows what to
            // do.
            return source.Provider.Execute(callExpression);
        }
        public static object Average(this IQueryable source, string member)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (member == null) throw new ArgumentNullException(nameof(member));

            // The most common variant of Queryable.Average() expects a lambda.
            // Since we just have a string to a property, we need to create a
            // lambda from the string in order to pass it to the sum method.

            // Lets create a ((TSource s) => s.Average ). First up, the parameter "s":
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");

            // Followed by accessing the Average property of "s" (s.Average):
            PropertyInfo property = source.ElementType.GetProperty(member);
            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);

            // And finally, we create a lambda from that. First specifying on what
            // to execute when the lambda is called, and finally the parameters of the lambda.
            Expression selector = Expression.Lambda(getter, parameter);

            // There are a lot of Queryable.Average() overloads with different
            // return types  (double, int, decimal, double?, int?, etc...).
            // We're going to find one that matches the type of our property.
            MethodInfo avgMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Average)
                     && m.ReturnType == property.PropertyType
                     && m.GetParameters().Length==2
                     && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments()[1]==property.PropertyType
                     && m.IsGenericMethod);

            // Now that we have the correct method, we need to know how to call the method.
            // Note that the Queryable.Sum<TSource>(source, selector) has a generic type,
            // which we haven't resolved yet. Good thing is that we can use copy the one from
            // our initial source expression.
            var genericAvgMethod = avgMethod.MakeGenericMethod(new[] {source.ElementType});

            // TSource, source and selector are now all resolved. We now know how to call
            // the sum-method. We're not going to call it here, we just express how we're going
            // call it.
            var callExpression = Expression.Call(
                null,
                genericAvgMethod,
                new[] {source.Expression, Expression.Quote(selector)});

            // Pass it down to the query provider. This can be a simple LinqToObject-datasource,
            // but also a more complex datasource (such as LinqToSql). Anyway, it knows what to
            // do.
            return source.Provider.Execute(callExpression);
        }
        public static object Max(this IQueryable source, string member)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (member == null) throw new ArgumentNullException(nameof(member));

            // The most common variant of Queryable.Max() expects a lambda.
            // Since we just have a string to a property, we need to create a
            // lambda from the string in order to pass it to the sum method.

            // Lets create a ((TSource s) => s.Max ). First up, the parameter "s":
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");

            // Followed by accessing the Max property of "s" (s.Max):
            PropertyInfo property = source.ElementType.GetProperty(member);
            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);

            // And finally, we create a lambda from that. First specifying on what
            // to execute when the lambda is called, and finally the parameters of the lambda.
            LambdaExpression selector = Expression.Lambda(getter, parameter);

            // There are a lot of Queryable.Max() overloads with different
            // return types  (double, int, decimal, double?, int?, etc...).
            // We're going to find one that matches the type of our property.
            MethodInfo maxMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Max)
                     && m.GetParameters().Length==2 && typeof(Expression).IsAssignableFrom(m.GetParameters()[1].ParameterType)
                     && m.IsGenericMethod);

            // Now that we have the correct method, we need to know how to call the method.
            // Note that the Queryable.Sum<TSource>(source, selector) has a generic type,
            // which we haven't resolved yet. Good thing is that we can use copy the one from
            // our initial source expression.
            var genericMaxMethod = maxMethod.MakeGenericMethod(new[] {source.ElementType,selector.Body.Type});

            // TSource, source and selector are now all resolved. We now know how to call
            // the sum-method. We're not going to call it here, we just express how we're going
            // call it.
            var callExpression = Expression.Call(
                null,
                genericMaxMethod,
                new[] {source.Expression, Expression.Quote(selector)});

            // Pass it down to the query provider. This can be a simple LinqToObject-datasource,
            // but also a more complex datasource (such as LinqToSql). Anyway, it knows what to
            // do.
            return source.Provider.Execute(callExpression);
        }
        public static object Min(this IQueryable source, string member)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (member == null) throw new ArgumentNullException(nameof(member));

            // The most common variant of Queryable.Min() expects a lambda.
            // Since we just have a string to a property, we need to create a
            // lambda from the string in order to pass it to the sum method.

            // Lets create a ((TSource s) => s.Min ). First up, the parameter "s":
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "s");

            // Followed by accessing the Price property of "s" (s.Min):
            PropertyInfo property = source.ElementType.GetProperty(member);
            MemberExpression getter = Expression.MakeMemberAccess(parameter, property);

            // And finally, we create a lambda from that. First specifying on what
            // to execute when the lambda is called, and finally the parameters of the lambda.
            LambdaExpression selector = Expression.Lambda(getter, parameter);

            // There are a lot of Queryable.Min() overloads with different
            // return types  (double, int, decimal, double?, int?, etc...).
            // We're going to find one that matches the type of our property.
            MethodInfo minMethod = typeof(Queryable).GetMethods().First(
                m => m.Name == nameof(Queryable.Min)
                     && m.GetParameters().Length==2 &&typeof(Expression).IsAssignableFrom(m.GetParameters()[1].ParameterType)
                     && m.IsGenericMethod);

            // Now that we have the correct method, we need to know how to call the method.
            // Note that the Queryable.Sum<TSource>(source, selector) has a generic type,
            // which we haven't resolved yet. Good thing is that we can use copy the one from
            // our initial source expression.
            var genericMinMethod = minMethod.MakeGenericMethod(new[] {source.ElementType,selector.Body.Type});

            // TSource, source and selector are now all resolved. We now know how to call
            // the sum-method. We're not going to call it here, we just express how we're going
            // call it.
            var callExpression = Expression.Call(
                null,
                genericMinMethod,
                new[] {source.Expression, Expression.Quote(selector)});

            // Pass it down to the query provider. This can be a simple LinqToObject-datasource,
            // but also a more complex datasource (such as LinqToSql). Anyway, it knows what to
            // do.
            return source.Provider.Execute(callExpression);
        }
    }
}