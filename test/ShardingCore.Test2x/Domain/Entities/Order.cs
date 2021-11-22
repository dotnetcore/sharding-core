using System;
using ShardingCore.Core;

namespace ShardingCore.Test2x.Domain.Entities
{
    public class Order:IShardingDataSource,IShardingTable
    {
        public Guid Id { get; set; }
        [ShardingDataSourceKey]
        public string Area { get; set; }
        public long Money { get; set; }
        [ShardingTableKey]
        public DateTime CreateTime { get; set; }
    }
}
