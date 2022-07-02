using System;
using System.Collections.Generic;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Test3x.Domain.Entities;
using ShardingCore.VirtualRoutes.Months;

namespace ShardingCore.Test3x.Shardings
{
   public  class OrderCreateTimeVirtualTableRoute:AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<Order>
    {
        //public override bool? EnableRouteParseCompileCache => true;
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }

        protected override List<string> CalcTailsOnStart()
        {
            var allTails = base.CalcTailsOnStart();
            allTails.Add("202112");
            return allTails;
        }

        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.CreateTime);
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
