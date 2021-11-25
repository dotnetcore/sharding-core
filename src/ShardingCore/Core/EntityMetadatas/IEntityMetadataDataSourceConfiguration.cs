using ShardingCore.Core.EntityMetadatas;

namespace ShardingCore.Core.EntityShardingMetadatas
{
    /// <summary>
    /// 对象元数据分库配置 用来配置分库对象的一些信息
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntityMetadataDataSourceConfiguration<TEntity> where TEntity:class
    {
        /// <summary>
        /// 配置分库对象
        /// </summary>
        /// <param name="builder"></param>
        void Configure(EntityMetadataDataSourceBuilder<TEntity> builder);
    }
}
