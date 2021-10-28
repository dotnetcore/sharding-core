using ShardingCore.Core.EntityMetadatas;

namespace ShardingCore.Core.EntityShardingMetadatas
{
    public interface IEntityMetadataDataSourceConfiguration<TEntity> where TEntity:class
    {
        void Configure(EntityMetadataDataSourceBuilder<TEntity> builder);
    }
}
