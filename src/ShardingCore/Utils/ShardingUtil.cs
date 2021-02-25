using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Core;
using ShardingCore.Core.Internal;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;

namespace ShardingCore.Utils
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 06 February 2021 08:28:07
* @Email: 326308290@qq.com
*/
    public class ShardingUtil
    {
        private static readonly ConcurrentDictionary<Type, ShardingEntityBaseType> _caches = new ConcurrentDictionary<Type, ShardingEntityBaseType>();

        private ShardingUtil()
        {
        }

        public static ShardingEntityBaseType Parse(Type entityType)
        {
            if (_caches.TryGetValue(entityType, out var baseType))
            {
                return baseType;
            }

            var isShardingDataSource = entityType.IsShardingDataSource();
            var isShardingTable = entityType.IsShardingEntity();
            baseType = new ShardingEntityBaseType()
            {
                EntityType = entityType,
                IsMultiDataSourceMapping = isShardingDataSource,
                IsMultiTableMapping = isShardingTable
            };


            if (!isShardingDataSource && isShardingTable)
                throw new NotSupportedException(entityType.ToString());

            PropertyInfo[] shardingProperties = entityType.GetProperties();


            if (isShardingTable)
            {
                var shardingTables = shardingProperties.SelectMany(p => p.GetCustomAttributes(true).Where(o => o.GetType() == typeof(ShardingKeyAttribute))).ToList();
                if (shardingTables.Count != 1)
                    throw new NotSupportedException($"{entityType}  From IShardingEntity should use single attribute [ShardingKey]");
            }

            var shardingDataSourceCount = 0;
            var shardingTableCount = 0;
            foreach (var shardingProperty in shardingProperties)
            {
                var attributes = shardingProperty.GetCustomAttributes(true);
                if (isShardingDataSource)
                {
                    if (attributes.FirstOrDefault(x => x.GetType() == typeof(ShardingDataSourceKeyAttribute)) is ShardingDataSourceKeyAttribute shardingDataSourceKey)
                    {
                        if (shardingDataSourceCount > 1)
                            throw new NotSupportedException($"{entityType}  From IShardingDataSource should use single attribute [{nameof(ShardingDataSourceKeyAttribute)}]");

                        baseType.ShardingDataSourceField = shardingProperty.Name;
                        shardingDataSourceCount++;
                    }
                }

                if (isShardingTable)
                {
                    if (attributes.FirstOrDefault(x => x.GetType() == typeof(ShardingKeyAttribute)) is ShardingKeyAttribute shardingKey)
                    {
                        if (shardingTableCount > 1)
                            throw new NotSupportedException($"{entityType}  From IShardingEntity should use single attribute [{nameof(ShardingKeyAttribute)}]");

                        baseType.ShardingTableField = shardingProperty.Name;
                        baseType.AutoCreateTable = shardingKey.AutoCreateTableOnStart == ShardingKeyAutoCreateTableEnum.UnKnown ? (bool?) null : (shardingKey.AutoCreateTableOnStart == ShardingKeyAutoCreateTableEnum.Create);
                        baseType.TailPrefix = shardingKey.TailPrefix;
                        shardingTableCount++;
                    }
                }
            }

            _caches.TryAdd(entityType, baseType);

            return baseType;
        }
        
        
        public static Func<IPhysicDataSource, bool> GetRouteDataSourceFilter<TKey>(IQueryable queryable, ShardingEntityBaseType shardingEntityBaseType, Func<object, TKey> shardingKeyConvert, Func<TKey, ShardingOperatorEnum, Expression<Func<IPhysicDataSource, bool>>> keyToTailExpression)
        {
            QueryableRouteShardingDataSourceDiscoverVisitor<TKey> visitor = new QueryableRouteShardingDataSourceDiscoverVisitor<TKey>(shardingEntityBaseType, shardingKeyConvert, keyToTailExpression);

            visitor.Visit(queryable.Expression);

            return visitor.GetDataSourceFilter();
        }

    }
}