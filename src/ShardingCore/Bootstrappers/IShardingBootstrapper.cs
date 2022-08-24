/*
* @Author: xjm
* @Description:
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrappers
{
    /// <summary>
    /// 主要的分片初始化器,需要手动调用,如果你的分片路由存在定时执行的job譬如
    /// 系统默认的时间分片的情况下那么需要调用<code>IShardingRuntimeContext初始化的时候会调用</code>
    /// </summary>
    public interface IShardingBootstrapper
    {
        void AutoShardingCreate();
    }
}