using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Sharding.PaginationConfigurations;

namespace Sample.SqlServer.Shardings
{
    public class SysUserModPaginationConfiguration : IPaginationConfiguration<SysUserMod>
    {
        public void Configure(PaginationBuilder<SysUserMod> builder)
        {
            builder.PaginationSequence(o => o.Age)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch);
            builder.ConfigReverseShardingPage(reverseTotalGe:900);
        }
    }
}
