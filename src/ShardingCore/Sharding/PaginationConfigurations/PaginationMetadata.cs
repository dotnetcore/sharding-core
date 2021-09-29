using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Sharding.PaginationConfigurations.MultiQueryPagination;

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
        /// 反向排序因子 skip>ReverseFactor * total 
        /// </summary>
        public double ReverseFactor { get; set; } = -1;

        /// <summary>
        /// 当条数大于ReverseTotalGe条后采用反向排序
        /// </summary>
        public long ReverseTotalGe { get; set; } = 10000L;
        /// <summary>
        /// 是否已开启反向排序  skip>ReverseFactor * total  查询条件必须存在 order by
        /// </summary>
        public bool EnableReverseShardingPage => ReverseFactor > 0 && ReverseFactor < 1 && ReverseTotalGe >= 500;

        /// <summary>
        /// 极度不规则分布时当分页最大一页书占全部的(UnevenFactorGe*100)%时启用内存排序
        /// </summary>
        [Obsolete]
        public double UnevenFactorGe { get; set; } = -1;
        /// <summary>
        /// 极度不规则分布时除了total最大一张表外的其余表相加不能超过UnevenLimit
        /// </summary>
        [Obsolete]
        public int UnevenLimit { get; set; } = 300;
        /// <summary>
        /// 启用不规则分布分页 查询条件必须存在 order by
        /// </summary>
        [Obsolete]
        public bool EnableUnevenShardingPage => UnevenFactorGe > 0 && UnevenFactorGe < 1 && UnevenLimit > 0;

        public IMultiQueryPredicate MultiQueryPredicate { get; set; }

        /// <summary>
        /// 是否启用多次查询
        /// </summary>
        public bool EnableMultiQuery => MultiQueryPredicate != null;

    }
}
