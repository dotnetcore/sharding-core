#if (NETCOREAPP3_0 || NETSTANDARD2_0) && SHARDINGCORE2_6
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

}
#endif