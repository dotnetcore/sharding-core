namespace ShardingCore.DbContexts.ShardingDbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/03/09 13:13:58
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */

    /// <summary>
    /// 
    /// </summary>
    public class ShardingTableScopeFactory:IShardingTableScopeFactory
    {
        private readonly IShardingTableAccessor _shardingTableAccessor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="shardingTableAccessor"></param>
        public ShardingTableScopeFactory(IShardingTableAccessor shardingTableAccessor)
        {
            _shardingTableAccessor = shardingTableAccessor;
        }
        /// <summary>
        /// 创建环境
        /// </summary>
        /// <returns></returns>
        public ShardingTableScope CreateScope()
        {
            _shardingTableAccessor.Context = null;
            return new ShardingTableScope(_shardingTableAccessor);
        }
    }
}