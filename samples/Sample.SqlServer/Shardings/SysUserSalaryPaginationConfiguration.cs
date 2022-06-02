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
                .UseRouteComparer(Comparer<string>.Default)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch);
            //设置当
            builder.PaginationSequence(o => o.DateOfMonth)
                .UseRouteComparer(Comparer<string>.Default)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch).UseAppendIfOrderNone(10);
            builder.PaginationSequence(o => o.Salary)
                .UseRouteComparer(Comparer<string>.Default)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch).UseAppendIfOrderNone();
            builder.ConfigReverseShardingPage(0.5d,10000L);
        }
    }
}
