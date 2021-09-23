using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.BulkConsole.DbContexts;
using Sample.BulkConsole.Entities;
using ShardingCore;
using ShardingCore.Extensions;

namespace Sample.BulkConsole
{
    class Program
    {
        public static readonly ILoggerFactory efLogger = LoggerFactory.Create(builder =>
        {
            builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
        });
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddShardingDbContext<MyShardingDbContext, MyDbContext>(
                o => o.UseSqlServer("Data Source=localhost;Initial Catalog=MyOrderSharding;Integrated Security=True;"))
                .Begin(o =>
                {
                    o.CreateShardingTableOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = true;
                })
                .AddShardingQuery((conStr, builder) => builder.UseSqlServer(conStr).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking))
                .AddShardingTransaction((connection, builder) => builder.UseSqlServer(connection))
                .AddDefaultDataSource("ds0", "Data Source=localhost;Initial Catalog=MyOrderSharding;Integrated Security=True;")
                .AddShardingTableRoute(op=> {
                    op.AddShardingTableRoute<OrderVirtualRoute>();
                }).End();
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IShardingBootstrapper>().Start();
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var myShardingDbContext = serviceScope.ServiceProvider.GetService<MyShardingDbContext>();
                if (!myShardingDbContext.Set<Order>().Any())
                {
                    var begin = DateTime.Now.Date.AddDays(-3);
                    var now = DateTime.Now;
                    var current = begin;
                    ICollection<Order> orders = new LinkedList<Order>();
                    int i = 0;
                    while (current < now)
                    {
                        orders.Add(new Order()
                        {
                            Id = i.ToString(),
                            OrderNo = $"orderno-" + i.ToString(),
                            Seq = i,
                            CreateTime = current
                        });
                        i++;
                        current = current.AddMilliseconds(100);
                    }

                    var startNew = Stopwatch.StartNew();
                    var bulkShardingEnumerable = myShardingDbContext.BulkShardingEnumerable(orders);
                    startNew.Stop();
                    Console.WriteLine($"订单总数:{i}条,myShardingDbContext.BulkShardingEnumerable(orders)用时:{startNew.ElapsedMilliseconds}毫秒");
                    startNew.Restart();
                    foreach (var dataSourceMap in bulkShardingEnumerable)
                    {
                        foreach (var tailMap in dataSourceMap.Value)
                        {
                            tailMap.Key.BulkInsert(tailMap.Value.ToList());
                        }
                    }
                    startNew.Stop();
                    Console.WriteLine($"订单总数:{i}条,myShardingDbContext.BulkInsert(orders)用时:{startNew.ElapsedMilliseconds}毫秒");

                    Console.WriteLine("ok");
                }

                var b = DateTime.Now.Date.AddDays(-3);
                var queryable = myShardingDbContext.Set<Order>().Where(o => o.CreateTime >= b).OrderBy(o => o.CreateTime);
                var startNew1 = Stopwatch.StartNew();

                int skip = 0, take = 10;
                for (int i = 1000; i < 20000; i++)
                {
                    skip = take * i;
                    startNew1.Restart();
                    var shardingPagedResult = queryable.ToShardingPage(i+1, take);
                    startNew1.Stop();
                        Console.WriteLine($"流式分页skip:[{skip}],take:[{take}]耗时用时:{startNew1.ElapsedMilliseconds}毫秒");
                }

                Console.WriteLine("ok");

            }
        }
    }
}
