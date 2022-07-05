namespace ShardingCore.Sharding
{
    public enum CreateDbContextStrategyEnum
    {
        /// <summary>
        /// 共享链接(只是用写链接字符串) 无需管理connection的生命周期
        /// 简单说就是无需调用dispose
        /// </summary>
        ShareConnection,
        /// <summary>
        /// 并行查询链接(有可能会使用读写分离链接字符串) 独立生命周期需要手动dispose或者等系统调用
        /// </summary>
        IndependentConnectionQuery,
        /// <summary>
        /// 并行写链接(只是用写链接字符串) 独立生命周期需要手动dispose或者等系统调用
        /// </summary>
        IndependentConnectionWrite
    }
}