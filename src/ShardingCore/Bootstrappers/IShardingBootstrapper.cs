/*
* @Author: xjm
* @Description:
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrappers
{
    /// <summary>
    /// 主要的分表初始化器,不再需要手动调用,<code>IShardingRuntimeContext初始化的时候会调用</code>
    /// </summary>
    public interface IShardingBootstrapper
    {
        void AutoShardingTable();
    }
}