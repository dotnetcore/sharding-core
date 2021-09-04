using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardingCore.Sharding.PaginationConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 7:45:16
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分页配置元数据
    /// </summary>
    public class  PaginationMetadata
    {
        public ISet<PaginationSequenceConfig> PaginationConfigs = new HashSet<PaginationSequenceConfig>();

        /// <summary>
        /// 反向排序因子
        /// </summary>
        public double ReverseFactor { get; set; } = -1;

        /// <summary>
        /// 当条数大于多少条后采用反向排序
        /// </summary>
        public long ReverseTotalGe { get; set; } = 10000L;
        /// <summary>
        /// 是否已开启反向排序 仅支持单排序
        /// </summary>
        public bool EnableReverseShardingPage => ReverseFactor > 0 && ReverseFactor < 1 && ReverseTotalGe >= 500;
        // /// <summary>
        // /// 当出现N张表分页需要跳过X条数据,获取Y条数据除了total条数最多的那张表以外的其他表和小于TakeInMemoryMaxRangeSkip那么就启用
        // /// </summary>
        // public int TakeInMemoryMaxRangeSkip { get; set; } = 1000;
        //
        // public bool EnableTakeInMemory(int skip)
        // {
        //     return skip > TakeInMemoryMaxRangeSkip && TakeInMemoryMaxRangeSkip > 500;
        // }

    }
}
