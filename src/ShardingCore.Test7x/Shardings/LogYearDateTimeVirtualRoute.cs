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
    public class LogYearDateTimeVirtualRoute:AbstractSimpleShardingYearKeyDateTimeVirtualTableRoute<LogYearDateTime>
    {
        //public override bool? EnableRouteParseCompileCache => true;
        protected override bool EnableHintRoute => true;

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override DateTime GetBeginTime()
        {
            return new DateTime(2020, 1, 1);
        }
        public override void Configure(EntityMetadataTableBuilder<LogYearDateTime> builder)
        {
            builder.ShardingProperty(o => o.LogTime);
        }
    }
}
