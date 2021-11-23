using System;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Test5x.Domain.Entities;
using ShardingCore.Test5x.Shardings.PaginationConfigs;
using ShardingCore.VirtualRoutes.Days;

namespace ShardingCore.Test5x.Shardings
{
    public class LogDayVirtualTableRoute:AbstractSimpleShardingDayKeyDateTimeVirtualTableRoute<LogDay>
    {
        protected override bool EnableHintRoute => true;
        protected override bool EnableAssertRoute => true;

        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }

        public override bool StartJob()
        {
            return true;
        }

        public override void Configure(EntityMetadataTableBuilder<LogDay> builder)
        {
            builder.ShardingProperty(o => o.LogTime);
            builder.TableSeparator(string.Empty);
        }

        public override IPaginationConfiguration<LogDay> CreatePaginationConfiguration()
        {
            return new LogDayPaginationConfiguration();
        }
    }
}
