using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Test6x.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string Area { get; set; }
        public long Money { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
