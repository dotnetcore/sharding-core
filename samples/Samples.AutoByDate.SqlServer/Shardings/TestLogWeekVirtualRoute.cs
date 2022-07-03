using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Samples.AutoByDate.SqlServer.Domain.Entities;
using Samples.AutoByDate.SqlServer.Domain.Maps;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Weeks;

namespace Samples.AutoByDate.SqlServer.Shardings
{
    public class TestLogWeekVirtualRoute:AbstractSimpleShardingWeekKeyDateTimeVirtualTableRoute<TestLogByWeek>
    {
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 8, 1);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override void Configure(EntityMetadataTableBuilder<TestLogByWeek> builder)
        {
            builder.ShardingProperty(o => o.CreateDate);
        }
    }
}
