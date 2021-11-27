using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Test2x.Domain.Entities;

namespace ShardingCore.Test2x.Shardings.PaginationConfigs
{
    public class LogDayPaginationConfiguration: IPaginationConfiguration<LogDay>
    {
        public void Configure(PaginationBuilder<LogDay> builder)
        {
            builder.PaginationSequence(o => o.LogTime)
                .UseQueryMatch(PaginationMatchEnum.Named | PaginationMatchEnum.Owner |
                               PaginationMatchEnum.PrimaryMatch).UseAppendIfOrderNone();
        }
    }
}
