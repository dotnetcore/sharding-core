namespace ShardingCore.Core.EntityMetadatas
{
    public interface IEntityMetadataAutoBindInitializer
    {
        void Initialize(EntityMetadata entityMetadata);
    }
}
