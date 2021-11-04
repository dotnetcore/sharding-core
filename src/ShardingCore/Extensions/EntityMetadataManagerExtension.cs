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
    }
}
