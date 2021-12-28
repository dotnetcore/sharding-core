using System;

namespace Sample.SqlServerShardingTable.Entities
{
    public class MultiShardingOrder
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsDelete { get; set; }
    }
}
