using System.Collections.Generic;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Sharding.EntityQueryConfigurations;

namespace Sample.SqlServer.Shardings
{
    public class SysUserSalaryEntityQueryConfiguration: IEntityQueryConfiguration<SysUserSalary>
    {
        public void Configure(EntityQueryBuilder<SysUserSalary> builder)
        {
            ////SysUserSalary表是按月分片,月份的排序字符串和int是一样的所以用默认的即可
            //#region 第一种仅配置后缀比较器或者不配置(默认就是字符串比较器)
            ////当前情况下只有Any All Contains会进行中断
            //builder.ShardingTailComparer(Comparer<string>.Default);
            //#endregion

            //#region 第二种配置后缀比较器并且配置排序相对于比较器的
            //builder.ShardingTailComparer(Comparer<string>.Default);
            ////DateOfMonth的排序和月份分片的后缀一致所以用true如果false,无果无关就不需要配置
            //builder.AddOrder(o => o.DateOfMonth, true);
            //#endregion

            //#region 第三种配置后缀比较器并且配置排序相对于比较器的
            //builder.ShardingTailComparer(Comparer<string>.Default);
            //builder.AddDefaultSequenceQueryTrip(false, CircuitBreakerMethodNameEnum.FirstOrDefault);
            //#endregion


            #region 第四种
            builder.ShardingTailComparer(Comparer<string>.Default, false);//表示他是倒叙
            //DateOfMonth的排序和月份分片的后缀一致所以用true如果false,无果无关就不需要配置
            builder.AddOrder(o => o.DateOfMonth, false);
            builder.AddDefaultSequenceQueryTrip(false, CircuitBreakerMethodNameEnum.FirstOrDefault, CircuitBreakerMethodNameEnum.Enumerator);
            #endregion



            builder.AddConnectionsLimit(2, LimitMethodNameEnum.First, LimitMethodNameEnum.FirstOrDefault, LimitMethodNameEnum.Any, LimitMethodNameEnum.LastOrDefault, LimitMethodNameEnum.Last, LimitMethodNameEnum.Max, LimitMethodNameEnum.Min);
            builder.AddConnectionsLimit(1, LimitMethodNameEnum.Enumerator);

        }
    }
}
