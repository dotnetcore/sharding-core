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
        public ISet<PaginationConfig> PaginationConfigs = new HashSet<PaginationConfig>();

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
        public bool EnableReverseShardingPage => ReverseFactor > 0 && ReverseFactor < 1 && ReverseTotalGe >= 1000;
        /// <summary>
        /// 分表发现如果少于多少条后直接取到内存 LESS THAN OR EQUAL
        /// </summary>
        public int TakeInMemoryCountIfLe { get; set; } = 100;

    }
}
