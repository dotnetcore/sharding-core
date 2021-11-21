using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core;

namespace ShardingCore.Test6x.Domain.Entities
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
