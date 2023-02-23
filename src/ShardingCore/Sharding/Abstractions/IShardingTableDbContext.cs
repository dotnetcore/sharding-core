using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 9:40:18
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 实现该接口让dbContext支持分表功能
    /// </summary>
    public interface IShardingTableDbContext
    {
        /// <summary>
        /// 无需实现
        /// </summary>
        IRouteTail RouteTail { get; set; }
    }

}
