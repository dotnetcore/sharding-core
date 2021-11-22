using System;
using System.Collections.Generic;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Test3x.Domain.Entities;
using ShardingCore.VirtualRoutes.Months;

namespace ShardingCore.Test3x.Shardings
{
   public  class OrderCreateTimeVirtualTableRoute:AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<Order>
    {
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }

        public override bool StartJob()
        {
            return true;
        }

        public override IPaginationConfiguration<Order> CreatePaginationConfiguration()
        {
            return new OrderCreateTimePaginationConfiguration();
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
