using System.Collections.Generic;
using ShardingCore.Core.Internal.Visitors;

namespace ShardingCore.Core.Internal.StreamMerge
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 25 January 2021 11:38:27
* @Email: 326308290@qq.com
*/
    internal class StreamMergeContext
    {
        public StreamMergeContext(int? skip, int? take, IEnumerable<PropertyOrder> orders = null)
        {
            Skip = skip;
            Take = take;
            Orders = orders??new List<PropertyOrder>();
        }

        public int? Skip { get; set; }
        public int? Take { get; set; }
        public IEnumerable<PropertyOrder> Orders { get; set; }
    }
}