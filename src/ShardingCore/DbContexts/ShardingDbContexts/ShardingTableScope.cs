using System;

namespace ShardingCore.DbContexts.ShardingDbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/03/09 00:00:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 
    /// </summary>
    public class ShardingTableScope: IDisposable
    {

        /// <summary>
        /// 分表配置访问器
        /// </summary>
        public IShardingTableAccessor ShardingTableAccessor { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="shardingAccessor"></param>
        public ShardingTableScope(IShardingTableAccessor shardingAccessor)
        {
            ShardingTableAccessor = shardingAccessor;
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Dispose()
        {
            ShardingTableAccessor.Context = null;
        }
    }
}