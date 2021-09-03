using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Sharding.PaginationConfigurations;

namespace Sample.SqlServer.Shardings
{
    public class SysUserSalaryPaginationConfiguration:IPaginationConfiguration<SysUserSalary>
    {
        public void Configure(PaginationBuilder<SysUserSalary> builder)
        {
            builder.PaginationSequence(o => o.Id)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch);
            builder.PaginationSequence(o => o.DateOfMonth)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch).UseAppendIfOrderNone();
            builder.PaginationSequence(o => o.Salary)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch).UseAppendIfOrderNone();
            builder.ConfigReverseShardingPage();
        }
    }
}
