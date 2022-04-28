using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

#if EFCORE3
using Microsoft.EntityFrameworkCore.Query.Internal;

#endif

namespace ShardingCore.Sharding
{
    internal class ShardingEntityFrameworkQueryableExtensions
    {
#if !EFCORE2
        public static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression? expression,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!(source.Provider is IAsyncQueryProvider provider))
                throw new InvalidOperationException(CoreStrings.IQueryableProviderNotAsync);
            if (operatorMethodInfo.IsGenericMethod)
            {
                MethodInfo methodInfo;
                if (operatorMethodInfo.GetGenericArguments().Length != 2)
                    methodInfo = operatorMethodInfo.MakeGenericMethod(typeof(TSource));
                else
                    methodInfo = operatorMethodInfo.MakeGenericMethod(typeof(TSource), ((IEnumerable<Type>)typeof(TResult).GetGenericArguments()).Single<Type>());
                operatorMethodInfo = methodInfo;
            }
            MethodInfo method = operatorMethodInfo;
            Expression[] expressionArray;
            if (expression != null)
                expressionArray = new Expression[2]
                {
                    source.Expression,
                    expression
                };
            else
                expressionArray = new Expression[1]
                {
                    source.Expression
                };
            MethodCallExpression methodCallExpression = Expression.Call((Expression)null, method, expressionArray);
            CancellationToken cancellationToken1 = cancellationToken;
            return provider.ExecuteAsync<TResult>((Expression)methodCallExpression, cancellationToken1);
        }
        public static TResult Execute<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression? expression)
        {
            if (!(source.Provider is IAsyncQueryProvider provider))
                throw new InvalidOperationException(CoreStrings.IQueryableProviderNotAsync);
            if (operatorMethodInfo.IsGenericMethod)
            {
                MethodInfo methodInfo;
                if (operatorMethodInfo.GetGenericArguments().Length != 2)
                    methodInfo = operatorMethodInfo.MakeGenericMethod(typeof(TSource));
                else
                    methodInfo = operatorMethodInfo.MakeGenericMethod(typeof(TSource), ((IEnumerable<Type>)typeof(TResult).GetGenericArguments()).Single<Type>());
                operatorMethodInfo = methodInfo;
            }
            MethodInfo method = operatorMethodInfo;
            Expression[] expressionArray;
            if (expression != null)
                expressionArray = new Expression[2]
                {
                    source.Expression,
                    expression
                };
            else
                expressionArray = new Expression[1]
                {
                    source.Expression
                };
            MethodCallExpression methodCallExpression = Expression.Call((Expression)null, method, expressionArray);
            return provider.Execute<TResult>((Expression)methodCallExpression);
        }

#endif

#if EFCORE2
        

    public static Task<TResult> ExecuteAsync<TSource, TResult>(
      MethodInfo operatorMethodInfo,
      IQueryable<TSource> source,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      if (!(source.Provider is IAsyncQueryProvider provider))
        throw new InvalidOperationException(CoreStrings.IQueryableProviderNotAsync);
      if (operatorMethodInfo.IsGenericMethod)
        operatorMethodInfo = operatorMethodInfo.MakeGenericMethod(typeof (TSource));
      MethodCallExpression methodCallExpression = Expression.Call((Expression) null, operatorMethodInfo, source.Expression);
      CancellationToken cancellationToken1 = cancellationToken;
      return provider.ExecuteAsync<TResult>((Expression) methodCallExpression, cancellationToken1);
    }
    public static TResult Execute<TSource, TResult>(
      MethodInfo operatorMethodInfo,
      IQueryable<TSource> source)
    {
      if (!(source.Provider is IAsyncQueryProvider provider))
        throw new InvalidOperationException(CoreStrings.IQueryableProviderNotAsync);
      if (operatorMethodInfo.IsGenericMethod)
        operatorMethodInfo = operatorMethodInfo.MakeGenericMethod(typeof (TSource));
      MethodCallExpression methodCallExpression = Expression.Call((Expression) null, operatorMethodInfo, source.Expression);
      return provider.Execute<TResult>((Expression) methodCallExpression);
    }
#endif
    }
}
