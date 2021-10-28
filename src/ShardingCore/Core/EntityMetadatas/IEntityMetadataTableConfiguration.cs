using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.EntityShardingMetadatas;

namespace ShardingCore.Core.EntityMetadatas
{
    public interface IEntityMetadataTableConfiguration<TEntity>where TEntity : class
    {
         void Configure(EntityMetadataTableBuilder<TEntity> builder);
    }
}
