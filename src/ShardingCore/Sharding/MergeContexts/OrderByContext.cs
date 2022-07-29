using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeContexts
{
    public sealed class OrderByContext
    {
        public LinkedList<PropertyOrder> PropertyOrders { get; } = new LinkedList<PropertyOrder>();
        public string GetOrderExpression()
        {
            return string.Join(",", PropertyOrders);
        }

        public override string ToString()
        {
            return string.Join(",", PropertyOrders.Select(o=>$"{o}"));
        }
    }
}
