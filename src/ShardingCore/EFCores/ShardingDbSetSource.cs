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
#if !EFCORE2 && !EFCORE3 && !EFCORE5 && !EFCORE6
    error
#endif
#if EFCORE5 || EFCORE6
    public class ShardingDbSetSource : IDbSetSource
    {

        private static readonly MethodInfo _genericCreateSet
            = typeof(ShardingDbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateSetFactory));

        private readonly ConcurrentDictionary<(Type Type, string Name), Func<DbContext, string, object>> _cache
            = new ConcurrentDictionary<(Type Type, string Name), Func<DbContext, string, object>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object Create(DbContext context, Type type)
            => CreateCore(context, type, null, _genericCreateSet);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object Create(DbContext context, string name, Type type)
            => CreateCore(context, type, name, _genericCreateSet);

        private object CreateCore(DbContext context, Type type, string name, MethodInfo createMethod)
            => _cache.GetOrAdd(
                (type, name),
                t => (Func<DbContext, string, object>)createMethod
                    .MakeGenericMethod(t.Type)
                    .Invoke(null, null))(context, name);

        private static Func<DbContext, string, object> CreateSetFactory<TEntity>()
            where TEntity : class
            => (c, name) => new ShardingInternalDbSet<TEntity>(c, name);
    }
#endif
#if EFCORE3
    public class ShardingDbSetSource:IDbSetSource
    {
        
        private static readonly MethodInfo _genericCreateSet
            = typeof(ShardingDbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateSetFactory));

        private readonly ConcurrentDictionary<Type, Func<DbContext, object>> _cache
            = new ConcurrentDictionary<Type, Func<DbContext, object>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object Create(DbContext context, Type type)
            => CreateCore(context, type, _genericCreateSet);

        private object CreateCore(DbContext context, Type type, MethodInfo createMethod)
            => _cache.GetOrAdd(
                type,
                t => (Func<DbContext, object>)createMethod
                    .MakeGenericMethod(t)
                    .Invoke(null, null))(context);

        private static Func<DbContext, object> CreateSetFactory<TEntity>()
            where TEntity : class
            => c => new ShardingInternalDbSet<TEntity>(c);
}
#endif

#if EFCORE2

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
#endif

}