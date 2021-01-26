using System.Collections.Generic;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 13:13:45
* @Email: 326308290@qq.com
*/
    internal class ExtraEntry
    {
        public ExtraEntry(int? skip, int? take, IEnumerable<PropertyOrder> orders)
        {
            Skip = skip;
            Take = take;
            Orders = orders;
        }

        public int? Skip { get; set; }
        public int? Take { get; set; }
        public IEnumerable<PropertyOrder> Orders { get; set; }
    }
}