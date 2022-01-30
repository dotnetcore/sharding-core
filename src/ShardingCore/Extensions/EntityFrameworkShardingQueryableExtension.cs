using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Core;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 30 January 2022 00:12:37
* @Email: 326308290@qq.com
*/
    public static class EntityFrameworkShardingQueryableExtension
    {
        internal static readonly MethodInfo NotSupportMethodInfo
            = typeof(EntityFrameworkShardingQueryableExtension).GetTypeInfo().GetDeclaredMethod(nameof(NotSupport)) ?? throw new InvalidOperationException($"not found method {nameof(NotSupport)}");

        public static IQueryable<TEntity> NotSupport<TEntity>(this IQueryable<TEntity> source)
        {
            Check.NotNull(source, nameof(source));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            (Expression)null,
                            NotSupportMethodInfo.MakeGenericMethod(typeof(TEntity)),
                            source.Expression))
                    // source.Provider.CreateQuery<TEntity>(
                    //     (Expression) Expression.Call((Expression) null, NotSupportMethodInfo.MakeGenericMethod(typeof (TEntity)), source.Expression))
                    : source;
        }
    }
}