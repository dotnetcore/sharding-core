using System;
using System.Collections.Generic;
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
        /// 配置生效当跳过多少条后  GREATER THAN OR EQUAL
        /// </summary>
        public long UseShardingPageIfGeSkipAvg { get; set; } = 3000L;
        /// <summary>
        /// 分表发现如果少于多少条后直接取到内存 LESS THAN OR EQUAL
        /// </summary>
        public int TakeInMemoryCountIfLe { get; set; } = 100;

    }
}
