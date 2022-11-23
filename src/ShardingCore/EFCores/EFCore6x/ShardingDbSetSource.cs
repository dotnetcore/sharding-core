#if EFCORE6
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

}
#endif