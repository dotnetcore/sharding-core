using ShardingCore.Core.ServiceProviders;

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 元数据和路由绑定初始化器
    /// </summary>
    public interface IEntityMetadataAutoBindInitializer
    {
        IShardingProvider RouteShardingProvider { get; }
        /// <summary>
        /// 初始化 在启动时会被调用并且将对象元数据绑定到对应的路由上面
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <param name="shardingProvider"></param>
        void Initialize(EntityMetadata entityMetadata,IShardingProvider shardingProvider);
    }
}
