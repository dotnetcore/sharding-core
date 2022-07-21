using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Years;

namespace Sample.SqlServer.Shardings
{
    public class TestYearShardingVirtualTableRoute:AbstractSimpleShardingYearKeyDateTimeVirtualTableRoute<TestYearSharding>
    {
        public override DateTime GetBeginTime()
        {
            return new DateTime(2020, 1, 1);
        }
        

        public override void Configure(EntityMetadataTableBuilder<TestYearSharding> builder)
        {
            builder.ShardingProperty(o => o.CreateTIme);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }
    }
}
