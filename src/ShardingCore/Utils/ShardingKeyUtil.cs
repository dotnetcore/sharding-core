using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Core;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.Querys;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualTables;

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
        private static readonly ConcurrentDictionary<Type, ShardingEntityConfig> _caches = new ConcurrentDictionary<Type, ShardingEntityConfig>();

        private ShardingKeyUtil()
        {
        }

        public static ShardingEntityConfig Parse(Type entityType)
        {
            if (!typeof(IShardingEntity).IsAssignableFrom(entityType))
                throw new NotSupportedException(entityType.ToString());
            if (_caches.TryGetValue(entityType, out var shardingEntityConfig))
            {
                return shardingEntityConfig;
            }

            PropertyInfo[] shardingProperties = entityType.GetProperties();
            foreach (var shardingProperty in shardingProperties)
            {
                var attribbutes = shardingProperty.GetCustomAttributes(true);
                if (attribbutes.FirstOrDefault(x => x.GetType() == typeof(ShardingKeyAttribute)) is ShardingKeyAttribute shardingKeyAttribute)
                {
                    if (shardingEntityConfig != null)
                        throw new ArgumentException($"{entityType} found more than one [ShardingKeyAttribute]");
                    shardingEntityConfig = new ShardingEntityConfig()
                    {
                        ShardingEntityType = entityType,
                        ShardingField = shardingProperty.Name,
                        AutoCreateTable = shardingKeyAttribute.AutoCreateTableOnStart==ShardingKeyAutoCreateTableEnum.UnKnown?(bool?)null:(shardingKeyAttribute.AutoCreateTableOnStart==ShardingKeyAutoCreateTableEnum.Create),
                        TailPrefix = shardingKeyAttribute.TailPrefix
                    };
                    _caches.TryAdd(entityType, shardingEntityConfig);
                }
            }

            return shardingEntityConfig;
        }

        public static Func<string, bool> GetRouteShardingTableFilter<TKey>(IQueryable queryable, ShardingEntityConfig shardingConfig, Func<object, TKey> shardingKeyConvert, Func<TKey, ShardingOperatorEnum, Expression<Func<string, bool>>> keyToTailExpression)
        {
            QueryableRouteShardingTableDiscoverVisitor<TKey> visitor = new QueryableRouteShardingTableDiscoverVisitor<TKey>(shardingConfig, shardingKeyConvert, keyToTailExpression);

            visitor.Visit(queryable.Expression);

            return visitor.GetStringFilterTail();
        }


        public static ISet<Type> GetShardingEntitiesFilter(IQueryable queryable)
        {
            ShardingEntitiesVisitor visitor = new ShardingEntitiesVisitor();

            visitor.Visit(queryable.Expression);

            return visitor.GetShardingEntities();
        }
        public static ISet<Type> GetQueryEntitiesFilter(IQueryable queryable)
        {
            QueryEntitiesVisitor visitor = new QueryEntitiesVisitor();

            visitor.Visit(queryable.Expression);

            return visitor.GetQueryEntities();
        }

    }
}