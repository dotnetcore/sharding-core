using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Core;
using ShardingCore.Core.Internal;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.VirtualDatabase;
using ShardingCore.Core.VirtualRoutes;
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
        private static readonly ConcurrentDictionary<Type, ShardingEntityConfig> _caches = new ConcurrentDictionary<Type, ShardingEntityConfig>();

        private ShardingUtil()
        {
        }

        public static ShardingEntityConfig Parse(Type entityType)
        {
            if (_caches.TryGetValue(entityType, out var entityConfig))
            {
                return entityConfig;
            }

            var isShardingTable = entityType.IsShardingTable();
            var isShardingDataSource = entityType.IsShardingDataSource();
            entityConfig = new ShardingEntityConfig()
            {
                EntityType = entityType,
                IsMultiDataSourceMapping = isShardingDataSource,
                IsMultiTableMapping = isShardingTable
            };


            //if (!isShardingDataSource && isShardingTable)
            //    throw new NotSupportedException(entityType.FullName);

            PropertyInfo[] shardingProperties = entityType.GetProperties();

            //if (isShardingTable)
            //{
            //    var shardingTables = shardingProperties.SelectMany(p => p.GetCustomAttributes(true).Where(o => o.GetType() == typeof(ShardingTableKeyAttribute))).ToList();
            //    if (shardingTables.Count != 1)
            //        throw new NotSupportedException($"{entityType}  From IShardingTable should use single attribute [{nameof(ShardingTableKeyAttribute)}]");
            //}

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

                        entityConfig.ShardingDataSourceField = shardingProperty.Name;
                        entityConfig.AutoCreateDataSource = shardingDataSourceKey.AutoCreateDataSourceOnStart == ShardingKeyAutoCreateDataSourceEnum.UnKnown ? (bool?)null : (shardingDataSourceKey.AutoCreateDataSourceOnStart == ShardingKeyAutoCreateDataSourceEnum.Create);
                        shardingDataSourceCount++;
                    }
                }

                if (isShardingTable)
                {
                    if (attributes.FirstOrDefault(x => x.GetType() == typeof(ShardingTableKeyAttribute)) is ShardingTableKeyAttribute shardingKey)
                    {
                        if (shardingTableCount > 1)
                            throw new NotSupportedException($"{entityType}  From IShardingTable should use single attribute [{nameof(ShardingTableKeyAttribute)}]");

                        entityConfig.ShardingTableField = shardingProperty.Name;
                        entityConfig.AutoCreateTable = shardingKey.AutoCreateTableOnStart == ShardingKeyAutoCreateTableEnum.UnKnown ? (bool?) null : (shardingKey.AutoCreateTableOnStart == ShardingKeyAutoCreateTableEnum.Create);
                        entityConfig.TailPrefix = shardingKey.TailPrefix;
                        shardingTableCount++;
                    }
                }
            }

            _caches.TryAdd(entityType, entityConfig);

            return entityConfig;
        }
        
        
        public static Func<string, bool> GetRouteDataSourceFilter<TKey>(IQueryable queryable, ShardingEntityConfig shardingEntityBaseType, Func<object, TKey> shardingKeyConvert, Func<TKey, ShardingOperatorEnum, Expression<Func<string, bool>>> keyToTailExpression)
        {
            QueryableRouteShardingDataSourceDiscoverVisitor<TKey> visitor = new QueryableRouteShardingDataSourceDiscoverVisitor<TKey>(shardingEntityBaseType, shardingKeyConvert, keyToTailExpression);

            visitor.Visit(queryable.Expression);

            return visitor.GetDataSourceFilter();
        }

    }
}