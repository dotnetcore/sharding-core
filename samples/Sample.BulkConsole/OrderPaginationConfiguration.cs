using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sample.BulkConsole.Entities;
using ShardingCore.Sharding.PaginationConfigurations;

namespace Sample.BulkConsole
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/8 10:29:22
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class OrderPaginationConfiguration:IPaginationConfiguration<Order>
    {
        public void Configure(PaginationBuilder<Order> builder)
        {
            builder.PaginationSequence(o => o.CreateTime)
                .UseRouteComparer(Comparer<string>.Default)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch).UseAppendIfOrderNone();
            builder.ConfigReverseShardingPage(0.5d, 10000L);
        }
    }
}
