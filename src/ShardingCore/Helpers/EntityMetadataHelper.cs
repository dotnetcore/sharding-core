using System;
using System.Linq;
using System.Reflection;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;

namespace ShardingCore.Helpers
{
    public class EntityMetadataHelper
    {
        private EntityMetadataHelper()
        {

        }

        public static void Configure<TEntity>(EntityMetadataTableBuilder<TEntity> builder) where TEntity : class
        {
            var entityType = typeof(TEntity);
            if (typeof(IShardingTable).IsAssignableFrom(entityType))
            {

                PropertyInfo[] shardingProperties = entityType.GetProperties();
                var shardingTableCount = 0;
                foreach (var shardingProperty in shardingProperties)
                {
                    var attributes = shardingProperty.GetCustomAttributes(true);
                    if (attributes.FirstOrDefault(x => x.GetType() == typeof(ShardingTableKeyAttribute)) is ShardingTableKeyAttribute shardingKey)
                    {
                        if (shardingTableCount > 1)
                            throw new NotSupportedException($"{entityType}  impl {nameof(IShardingTable)}  should use single attribute [{nameof(ShardingTableKeyAttribute)}]");

                        builder.ShardingProperty(shardingProperty.Name);
                        var autoCreateTable =
                            shardingKey.AutoCreateTableOnStart == ShardingKeyAutoCreateTableEnum.UnKnown
                                ? (bool?) null
                                : (shardingKey.AutoCreateTableOnStart == ShardingKeyAutoCreateTableEnum.Create);
                        builder.AutoCreateTable(autoCreateTable);
                        builder.TableSeparator(shardingKey.TableSeparator);
                        shardingTableCount++;
                    }
                }
            }
        }
        public static void Configure<TEntity>(EntityMetadataDataSourceBuilder<TEntity> builder) where TEntity : class
        {
            var entityType = typeof(TEntity);
            if (typeof(IShardingDataSource).IsAssignableFrom(entityType))
            {

                PropertyInfo[] shardingProperties = entityType.GetProperties();


                var shardingDataSourceCount = 0;
                foreach (var shardingProperty in shardingProperties)
                {
                    var attributes = shardingProperty.GetCustomAttributes(true);
                    if (attributes.FirstOrDefault(x => x.GetType() == typeof(ShardingDataSourceKeyAttribute)) is ShardingDataSourceKeyAttribute shardingDataSourceKey)
                    {
                        if (shardingDataSourceCount > 1)
                            throw new NotSupportedException($"{entityType}  impl {nameof(IShardingDataSource)} should use single attribute [{nameof(ShardingDataSourceKeyAttribute)}]");
                        builder.ShardingProperty(shardingProperty.Name);

                        var autoCreateDataSource = shardingDataSourceKey.AutoCreateDataSourceTableOnStart ==
                            ShardingKeyAutoCreateTableEnum.UnKnown
                                ? (bool?)null
                                : (shardingDataSourceKey.AutoCreateDataSourceTableOnStart ==
                                   ShardingKeyAutoCreateTableEnum.Create);

                        builder.AutoCreateDataSource(autoCreateDataSource);
                        shardingDataSourceCount++;
                    }

                }
            }
        }
    }
}
