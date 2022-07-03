using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sample.SqlServerShardingTable.Common;
using Sample.SqlServerShardingTable.Entities;
using ShardingCore;
using ShardingCore.Bootstrappers;

namespace Sample.SqlServerShardingTable
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
                    if (!myDbContext.Set<Setting>().Any())
                {
                    List<Setting> settings = new List<Setting>(3);
                    settings.Add(new Setting()
                    {
                        Code = "Admin",
                        Name = "AdminName"
                    });
                    settings.Add(new Setting()
                    {
                        Code = "User",
                        Name = "UserName"
                    });
                    settings.Add(new Setting()
                    {
                        Code = "SuperAdmin",
                        Name = "SuperAdminName"
                    });
                    string[] areas = new string[] {"A","B","C" };
                    List<SysUser> users = new List<SysUser>(10);
                    for (int i = 0; i < 10; i++)
                    {
                        var uer=new SysUser()
                        {
                            Id = i.ToString(),
                            Name = $"MyName{i}",
                            SettingCode = settings[i % 3].Code,
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

                    var multiShardingOrders = new List<MultiShardingOrder>(9);
                    #region 添加多字段分表

                    {
                        var now = new DateTime(2021, 10, 1, 13, 13, 11);
                        multiShardingOrders.Add(new MultiShardingOrder()
                        {
                            Id = 231765457240207360,
                            Name = $"{now:yyyy/MM/dd HH:mm:ss}",
                            CreateTime = now
                        });
                    }
                    {
                        var now = new DateTime(2021, 10, 2, 11, 3, 11);
                        multiShardingOrders.Add(new MultiShardingOrder()
                        {
                            Id = 232095129534607360,
                            Name = $"{now:yyyy/MM/dd HH:mm:ss}",
                            CreateTime = now
                        });
                    }
                    {
                        var now = new DateTime(2021, 10, 3, 7, 7, 7);
                        multiShardingOrders.Add(new MultiShardingOrder()
                        {
                            Id = 232398109278351360,
                            Name = $"{now:yyyy/MM/dd HH:mm:ss}",
                            CreateTime = now
                        });
                    }
                    {
                        var now = new DateTime(2021, 11, 6, 13, 13, 11);
                        multiShardingOrders.Add(new MultiShardingOrder()
                        {
                            Id = 244811420401807360,
                            Name = $"{now:yyyy/MM/dd HH:mm:ss}",
                            CreateTime = now
                        });
                    }
                    {
                        var now = new DateTime(2021, 11, 21, 19, 43, 0);
                        multiShardingOrders.Add(new MultiShardingOrder()
                        {
                            Id = 250345338962063360,
                            Name = $"{now:yyyy/MM/dd HH:mm:ss}",
                            CreateTime = now
                        });
                    }
                    {
                        var now = new DateTime(2021, 12, 5, 5, 5, 11);
                        multiShardingOrders.Add(new MultiShardingOrder()
                        {
                            Id = 255197859283087360,
                            Name = $"{now:yyyy/MM/dd HH:mm:ss}",
                            CreateTime = now
                        });
                    }
                    {
                        var now = new DateTime(2021, 12, 9, 19, 13, 11);
                        multiShardingOrders.Add(new MultiShardingOrder()
                        {
                            Id = 256860816933007360,
                            Name = $"{now:yyyy/MM/dd HH:mm:ss}",
                            CreateTime = now
                        });
                    }
                    {
                        var now = new DateTime(2021, 12, 19, 13, 13, 11);
                        multiShardingOrders.Add(new MultiShardingOrder()
                        {
                            Id = 260394098622607360,
                            Name = $"{now:yyyy/MM/dd HH:mm:ss}",
                            CreateTime = now
                        });
                    } 
                    #endregion

                    myDbContext.AddRange(settings);
                    myDbContext.AddRange(users);
                    myDbContext.AddRange(orders);
                    myDbContext.AddRange(multiShardingOrders);
                    myDbContext.SaveChanges();
                }
            }
        }
    }
}
