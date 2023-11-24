using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Test.Domain.Entities;

namespace ShardingCore.Test.Shardings.PaginationConfigs
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
