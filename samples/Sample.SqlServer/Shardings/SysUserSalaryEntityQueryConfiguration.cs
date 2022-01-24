using System.Collections.Generic;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Sharding.EntityQueryConfigurations;

namespace Sample.SqlServer.Shardings
{
    public class SysUserSalaryEntityQueryConfiguration: IEntityQueryConfiguration<SysUserSalary>
    {
        public void Configure(EntityQueryBuilder<SysUserSalary> builder)
        {
            builder.AddOrder(o => o.DateOfMonth);
            builder.AddConnectionsLimit(2, QueryableMethodNameEnum.First, QueryableMethodNameEnum.FirstOrDefault,QueryableMethodNameEnum.Any);
        }
    }
}
