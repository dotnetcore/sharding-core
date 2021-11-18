using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.EntityMetadatas;

namespace ShardingCore.Extensions
{
    public static class EntityMetadataManagerExtension
    {
        public static EntityMetadata TryGet<TEntity>(this IEntityMetadataManager entityMetadataManager)
            where TEntity : class
        {
            return entityMetadataManager.TryGet(typeof(TEntity));
        }

        public static bool IsShardingTable<TEntity>(this IEntityMetadataManager entityMetadataManager) where TEntity : class
        {
            return entityMetadataManager.IsShardingTable(typeof(TEntity));
        }
        public static bool IsShardingDataSource<TEntity>(this IEntityMetadataManager entityMetadataManager) where TEntity : class
        {
            return entityMetadataManager.IsShardingDataSource(typeof(TEntity));
        }
    }
}
