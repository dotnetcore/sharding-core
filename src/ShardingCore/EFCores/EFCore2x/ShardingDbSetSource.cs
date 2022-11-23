#if EFCORE2
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 10:17:43
    * @Email: 326308290@qq.com
    */

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ShardingDbSetSource : IDbSetSource, IDbQuerySource
    {
        private static readonly MethodInfo _genericCreateSet
            = typeof(ShardingDbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateSetFactory));

        private static readonly MethodInfo _genericCreateQuery
            = typeof(ShardingDbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateQueryFactory));

        private readonly ConcurrentDictionary<Type, Func<DbContext, object>> _cache
            = new ConcurrentDictionary<Type, Func<DbContext, object>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object Create(DbContext context, Type type)
            => CreateCore(context, type, _genericCreateSet);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateQuery(DbContext context, Type type)
            => CreateCore(context, type, _genericCreateQuery);

        private object CreateCore(DbContext context, Type type, MethodInfo createMethod)
            => _cache.GetOrAdd(
                type,
                t => (Func<DbContext, object>)createMethod
                    .MakeGenericMethod(t)
                    .Invoke(null, null))(context);

        private static Func<DbContext, object> CreateSetFactory<TEntity>()
            where TEntity : class
            => c => new ShardingInternalDbSet<TEntity>(c);

        private static Func<DbContext, DbQuery<TQuery>> CreateQueryFactory<TQuery>()
            where TQuery : class
            => c => new ShardingInternalDbQuery<TQuery>(c);
    }
}
#endif