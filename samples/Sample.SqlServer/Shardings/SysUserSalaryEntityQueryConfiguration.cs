using System.Collections.Generic;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Sharding.EntityQueryConfigurations;

namespace Sample.SqlServer.Shardings
{
    public class SysUserSalaryEntityQueryConfiguration: IEntityQueryConfiguration<SysUserSalary>
    {
        public void Configure(EntityQueryBuilder<SysUserSalary> builder)
        {
            //当前表示按月分片,月份的排序字符串和int是一样的所以用某人的即可
            builder.ShardingTailComparer(Comparer<string>.Default);
            //DateOfMonth的排序和月份分片的后缀一致所以用true如果false,无果无关就不需要配置
            builder.AddOrder(o => o.DateOfMonth,true);
            builder.AddConnectionsLimit(2, QueryableMethodNameEnum.First, QueryableMethodNameEnum.FirstOrDefault,QueryableMethodNameEnum.Any,QueryableMethodNameEnum.LastOrDefault,QueryableMethodNameEnum.Last);

            builder.AddDefaultSequenceQueryTrip(false, QueryableMethodNameEnum.FirstOrDefault);
        }
    }
}
