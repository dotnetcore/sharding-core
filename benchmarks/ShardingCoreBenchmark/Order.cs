using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore6x
{
    internal class Order
    {
        public string Id { get; set; }
        public long Amount { get; set; }
        public string Body { get; set; } 
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
        public string Payer { get; set; }

        public OrderStatusEnum OrderStatus { get; set; }
    }

    public enum OrderStatusEnum
    {
        NotPay=1,
        Succeed=1<<1,
        Failed=1<<2,
    }
}
