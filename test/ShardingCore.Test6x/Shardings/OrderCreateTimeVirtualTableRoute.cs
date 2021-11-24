using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Test6x.Domain.Entities;
using ShardingCore.VirtualRoutes.Months;

namespace ShardingCore.Test6x.Shardings
{
   public  class OrderCreateTimeVirtualTableRoute:AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<Order>
    {
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }

        public override IPaginationConfiguration<Order> CreatePaginationConfiguration()
        {
            return new OrderCreateTimePaginationConfiguration();
        }
        public override bool AutoCreateTableByTime()
        {
            return true;
        }
    }

   public class OrderCreateTimePaginationConfiguration : IPaginationConfiguration<Order>
   {
       public void Configure(PaginationBuilder<Order> builder)
       {
           builder.PaginationSequence(o => o.CreateTime)
               .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch)
               .UseAppendIfOrderNone().UseRouteComparer(Comparer<string>.Default);
       }
   }
}
