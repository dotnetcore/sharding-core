using System;
using ShardingCore.Core;

namespace ShardingCore.Test3x.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        [ShardingDataSourceKey]
        public string Area { get; set; }
        public long Money { get; set; }
        [ShardingTableKey]
        public DateTime CreateTime { get; set; }
    }
}
