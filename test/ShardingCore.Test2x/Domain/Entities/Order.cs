using System;
using ShardingCore.Core;

namespace ShardingCore.Test2x.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string Area { get; set; }
        public long Money { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
