namespace ShardingCore.Bootstrappers
{
    /// <summary>
    /// 分片初始化器主要用来初始化元数据信息和平行表
    /// </summary>
    internal interface IShardingInitializer
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
    }
}