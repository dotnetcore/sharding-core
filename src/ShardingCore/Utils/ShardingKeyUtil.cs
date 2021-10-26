using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using ShardingCore.Core;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.Querys;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;

namespace ShardingCore.Utils
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 19 December 2020 20:20:29
    * @Email: 326308290@qq.com
    */
    public class ShardingKeyUtil
    {
        private static readonly ConcurrentDictionary<Type, IKey> _caches = new ConcurrentDictionary<Type, IKey>();

        private ShardingKeyUtil()
        {
        }

        public static IKey ParsePrimaryKeyName(IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            _caches.TryAdd(entityType.ClrType, primaryKey);
            return primaryKey;
        }

        public static IEnumerable<object> GetPrimaryKeyValues(object entity)
        {
            var entityType = entity.GetType();
            if (!_caches.TryGetValue(entityType, out var primaryKey))
            {
                return null;
            }

            return primaryKey.Properties.Select(o => entity.GetPropertyValue(o.Name));
        }
        public static IKey GetEntityIKey(object entity)
        {
            var entityType = entity.GetType();
            if (!_caches.TryGetValue(entityType, out var primaryKey))
            {
                return null;
            }

            return primaryKey;
        }
        public static IKey GetEntityIKey(Type entityType)
        {
            if (!_caches.TryGetValue(entityType, out var primaryKey))
            {
                return null;
            }

            return primaryKey;
        }



        //public static ISet<Type> GetShardingEntitiesFilter(IQueryable queryable)
        //{
        //    ShardingEntitiesVisitor visitor = new ShardingEntitiesVisitor();

        //    visitor.Visit(queryable.Expression);

        //    return visitor.GetShardingEntities();
        //}

    }
}