using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sample.SqlServerShardingDataSource.Entities;
using ShardingCore;
using ShardingCore.Bootstrappers;

namespace Sample.SqlServerShardingDataSource
{
    public static class StartupExtension
    {
        public static void UseShardingCore(this IApplicationBuilder app)
        {
            app.ApplicationServices.UseAutoShardingCreate();
            app.ApplicationServices.UseAutoTryCompensateTable();
        }
        public static void InitSeed(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var myDbContext = serviceScope.ServiceProvider.GetRequiredService<MyDbContext>();
                var any = myDbContext.Set<SysUser>().Where(o => o.Area == "A").Any();
                if (!myDbContext.Set<SysUser>().Any())
                {
                    string[] areas = new string[] {"A","B","C" };
                    List<SysUser> users = new List<SysUser>(10);
                    for (int i = 0; i < 10; i++)
                    {
                        var uer=new SysUser()
                        {
                            Id = i.ToString(),
                            Name = $"MyName{i}",
                            Area = areas[i % 3]
                        };
                        users.Add(uer);
                    }
                    List<Order> orders = new List<Order>(300);
                    var begin = new DateTime(2021, 1, 1, 3, 3, 3);
                    for (int i = 0; i < 300; i++)
                    {

                        var order = new Order()
                        {
                            Id = i.ToString(),
                            Payer = $"{i % 10}",
                            Money = 100+new Random().Next(100,3000),
                            OrderStatus = (OrderStatusEnum)(i % 4 + 1),
                            Area = areas[i % 3],
                            CreationTime = begin.AddDays(i)
                        };
                        orders.Add(order);
                    }
                    myDbContext.AddRange(users);
                    myDbContext.AddRange(orders);
                    myDbContext.SaveChanges();
                }
            }
        }
    }
}
