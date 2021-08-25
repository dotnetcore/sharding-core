using ShardingCore.Core.VirtualRoutes.RouteTails.Abstractions;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 9:40:18
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingTableDbContext
    {
        IRouteTail RouteTail { get; set; }
    }
}
