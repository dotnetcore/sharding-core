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
        private static readonly ConcurrentDictionary<Type, string> _caches = new ConcurrentDictionary<Type, string>();

        private ShardingKeyUtil()
        {
        }

        public static void ParsePrimaryKeyName(IEntityType entityType)
        {
            var keyName = entityType.FindPrimaryKey().Properties
                .Select(x => x.Name).FirstOrDefault();
            _caches.TryAdd(entityType.ClrType, keyName);
        }

        public static object GetPrimaryKeyValue(object entity)
        {
            var entityType = entity.GetType();
            if (!_caches.TryGetValue(entityType, out var keyName))
            {
                return null;
            }

            return entity.GetPropertyValue(keyName);
        }



        //public static ISet<Type> GetShardingEntitiesFilter(IQueryable queryable)
        //{
        //    ShardingEntitiesVisitor visitor = new ShardingEntitiesVisitor();

        //    visitor.Visit(queryable.Expression);

        //    return visitor.GetShardingEntities();
        //}

    }
}