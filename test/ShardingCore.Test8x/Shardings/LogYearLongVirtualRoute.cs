using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test.Domain.Entities;
using ShardingCore.VirtualRoutes.Years;

namespace ShardingCore.Test.Shardings
{
    public class LogYearLongVirtualRoute:AbstractSimpleShardingYearKeyLongVirtualTableRoute<LogYearLong>
    {
        //public override bool? EnableRouteParseCompileCache => true;
        protected override bool EnableHintRoute => true;

        public override void Configure(EntityMetadataTableBuilder<LogYearLong> builder)
        {
            builder.ShardingProperty(o => o.LogTime);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }
    }
}
