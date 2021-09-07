using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 13:08:31
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public enum ReadStrategyEnum
    {
        Random=1,
        Loop=2,
    }

    public enum ReadConnStringGetStrategyEnum
    {
        /// <summary>
        /// 每次都是最新的
        /// </summary>
        LatestEveryTime,
        /// <summary>
        /// 已dbcontext作为缓存条件每次都是第一次获取的
        /// </summary>
        LatestFirstTime
    }
}
