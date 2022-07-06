using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShardingCore.Bootstrappers;
using ShardingCore.Helpers;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;
using ShardingCore.Test2x.Domain.Entities;
using ShardingCore.Test2x.Shardings;

namespace ShardingCore.Test2x
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 15 January 2021 15:37:46
    * @Email: 326308290@qq.com
    */
    public class Startup
    {
        public static readonly ILoggerFactory efLogger = LoggerFactory.Create(builder =>
        {
            builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
        });

        // 支持的形式：
        // ConfigureServices(IServiceCollection services)
        // ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        // ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        {
            services.AddShardingDbContext<ShardingDefaultDbContext>()
                .UseRouteConfig(op =>
                {
                    op.AddShardingDataSourceRoute<OrderAreaShardingVirtualDataSourceRoute>();
                    op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                    op.AddShardingTableRoute<SysUserSalaryVirtualTableRoute>();
                    op.AddShardingTableRoute<OrderCreateTimeVirtualTableRoute>();
                    op.AddShardingTableRoute<LogDayVirtualTableRoute>();
                    op.AddShardingTableRoute<LogWeekDateTimeVirtualTableRoute>();
                    op.AddShardingTableRoute<LogWeekTimeLongVirtualTableRoute>();
                    op.AddShardingTableRoute<LogYearDateTimeVirtualRoute>();
                    op.AddShardingTableRoute<LogMonthLongvirtualRoute>();
                    op.AddShardingTableRoute<LogYearLongVirtualRoute>();
                    op.AddShardingTableRoute<SysUserModIntVirtualRoute>();
                    op.AddShardingTableRoute<LogDayLongVirtualRoute>();
                    op.AddShardingTableRoute<MultiShardingOrderVirtualTableRoute>();

                })
                .UseConfig(op =>
                {

                    //当无法获取路由时会返回默认值而不是报错
                    op.ThrowIfQueryRouteNotMatch = false;
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
                    });

                    op.AddDefaultDataSource("A",
                        "Data Source=localhost;Initial Catalog=ShardingCoreDBA;Integrated Security=True;");
                    op.AddExtraDataSource(sp =>
                    {
                        return new Dictionary<string, string>()
                    {
                        { "B", "Data Source=localhost;Initial Catalog=ShardingCoreDBB;Integrated Security=True;" },
                        { "C", "Data Source=localhost;Initial Catalog=ShardingCoreDBC;Integrated Security=True;" },
                    };
                    });
                    op.AddReadWriteSeparation(sp =>
                    {
                        return new Dictionary<string, IEnumerable<string>>()
                    {
                        {
                            "A", new HashSet<string>()
                            {
                                "Data Source=localhost;Initial Catalog=ShardingCoreDBB;Integrated Security=True;"
                            }
                        }
                    };
                    }, ReadStrategyEnum.Loop, defaultEnable: false, readConnStringGetStrategy: ReadConnStringGetStrategyEnum.LatestEveryTime);
                })
                .ReplaceService<ITableEnsureManager,SqlServerTableEnsureManager>(ServiceLifetime.Singleton)
                .AddShardingCore();
            // services.AddShardingDbContext<ShardingDefaultDbContext, DefaultDbContext>(o => o.UseMySql(hostBuilderContext.Configuration.GetSection("MySql")["ConnectionString"],new MySqlServerVersion("5.7.15"))
            //     ,op =>
            //     {
            //         op.EnsureCreatedWithOutShardingTable = true;
            //         op.CreateShardingTableOnStart = true;
            //         op.UseShardingOptionsBuilder((connection, builder) => builder.UseMySql(connection,new MySqlServerVersion("5.7.15")).UseLoggerFactory(efLogger),
            //             (conStr,builder)=> builder.UseMySql(conStr,new MySqlServerVersion("5.7.15")).UseLoggerFactory(efLogger));
            //         op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
            //         op.AddShardingTableRoute<SysUserSalaryVirtualTableRoute>();
            //     });
        }

        // 可以添加要用到的方法参数，会自动从注册的服务中获取服务实例，类似于 asp.net core 里 Configure 方法
        public void Configure(IServiceProvider serviceProvider)
        {
            serviceProvider.UseAutoShardingCreate();
            serviceProvider.UseAutoTryCompensateTable();
            // 有一些测试数据要初始化可以放在这里
            InitData(serviceProvider).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 添加种子数据
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private async Task InitData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var virtualDbContext = scope.ServiceProvider.GetService<ShardingDefaultDbContext>();
                if (!await virtualDbContext.Set<SysUserMod>().AnyAsync())
                {
                    var ids = Enumerable.Range(1, 1000);
                    var userMods = new List<SysUserMod>();
                    var userModInts = new List<SysUserModInt>();
                    var userSalaries = new List<SysUserSalary>();
                    var beginTime = new DateTime(2020, 1, 1);
                    var endTime = new DateTime(2021, 12, 1);
                    foreach (var id in ids)
                    {
                        userMods.Add(new SysUserMod()
                        {
                            Id = id.ToString(),
                            Age = id,
                            Name = $"name_{id}",
                            AgeGroup = Math.Abs(id % 10)
                        });
                        userModInts.Add(new SysUserModInt()
                        {
                            Id = id,
                            Age = id,
                            Name = $"name_{id}",
                            AgeGroup = Math.Abs(id % 10)
                        });
                        var tempTime = beginTime;
                        var i = 0;
                        while (tempTime <= endTime)
                        {
                            var dateOfMonth = $@"{tempTime:yyyyMM}";
                            userSalaries.Add(new SysUserSalary()
                            {
                                Id = $@"{id}{dateOfMonth}",
                                UserId = id.ToString(),
                                DateOfMonth = int.Parse(dateOfMonth),
                                Salary = 700000 + id * 100 * i,
                                SalaryLong = 700000 + id * 100 * i,
                                SalaryDecimal = (700000 + id * 100 * i) / 100m,
                                SalaryDouble = (700000 + id * 100 * i) / 100d,
                                SalaryFloat = (700000 + id * 100 * i) / 100f
                            });
                            tempTime = tempTime.AddMonths(1);
                            i++;
                        }
                    }

                    var areas = new List<string>(){"A","B","C"};
                    List<Order> orders = new List<Order>(360);
                    var begin = new DateTime(2021, 1, 1);
                    for (int i = 0; i < 320; i++)
                    {
                        orders.Add(new Order()
                        {
                            Id = Guid.NewGuid(),
                            Area = areas[i%3],
                            CreateTime = begin,
                            Money = i
                        });
                        begin = begin.AddDays(1);
                    }

                    List<LogDay> logDays = new List<LogDay>(3600);
                    List<LogDayLong> logDayLongs = new List<LogDayLong>(3600);

                    var levels = new List<string>(){"info","warning","error"};
                    var begin1 = new DateTime(2021, 1, 1);
                    for (int i = 0; i < 300; i++)
                    {
                        var ltime = begin1;
                        for (int j = 0; j < 10; j++)
                        {
                            logDays.Add(new LogDay()
                            {
                                Id = Guid.NewGuid(),
                                LogLevel = levels[j%3],
                                LogBody = $"{i}_{j}",
                                LogTime = ltime.AddHours(1)
                            });
                            logDayLongs.Add(new LogDayLong()
                            {
                                Id = Guid.NewGuid(),
                                LogLevel = levels[j%3],
                                LogBody = $"{i}_{j}",
                                LogTime = ShardingCoreHelper.ConvertDateTimeToLong(ltime.AddHours(1))
                            });
                            ltime = ltime.AddHours(1);
                        }
                        begin1 = begin1.AddDays(1);
                    }

                    List<LogWeekDateTime> logWeeks = new List<LogWeekDateTime>(300);
                    var begin2 = new DateTime(2021,1,1);
                    for (int i = 0; i < 300; i++)
                    {
                        logWeeks.Add(new LogWeekDateTime()
                        {
                            Id = Guid.NewGuid().ToString("n"),
                            Body = $"body_{i}",
                            LogTime = begin2
                        });
                        begin2 = begin2.AddDays(1);
                    }
                    List<LogWeekTimeLong> logWeekLongs = new List<LogWeekTimeLong>(300);
                    var begin3 = new DateTime(2021,1,1);
                    for (int i = 0; i < 300; i++)
                    {
                        logWeekLongs.Add(new LogWeekTimeLong()
                        {
                            Id = Guid.NewGuid().ToString("n"),
                            Body = $"body_{i}",
                            LogTime = ShardingCoreHelper.ConvertDateTimeToLong(begin3)
                        });
                        begin3 = begin3.AddDays(1);
                    }
                    List<LogYearDateTime> logYears = new List<LogYearDateTime>(600);
                    var begin4 = new DateTime(2020,1,1);
                    for (int i = 0; i < 600; i++)
                    {
                        logYears.Add(new LogYearDateTime()
                        {
                            Id = Guid.NewGuid(),
                            LogBody = $"body_{i}",
                            LogTime = begin4
                        });
                        begin4 = begin4.AddDays(1);
                    }


                    List<LogMonthLong> logMonthLongs = new List<LogMonthLong>(300);
                    var begin5 = new DateTime(2021, 1, 1);
                    for (int i = 0; i < 300; i++)
                    {
                        logMonthLongs.Add(new LogMonthLong()
                        {
                            Id = Guid.NewGuid().ToString("n"),
                            Body = $"body_{i}",
                            LogTime = ShardingCoreHelper.ConvertDateTimeToLong(begin5)
                        });
                        begin5 = begin5.AddDays(1);
                    }

                    List<LogYearLong> logYearkLongs = new List<LogYearLong>(300);
                    var begin6 = new DateTime(2021, 1, 1);
                    for (int i = 0; i < 300; i++)
                    {
                        logYearkLongs.Add(new LogYearLong()
                        {
                            Id = Guid.NewGuid().ToString("n"),
                            LogBody = $"body_{i}",
                            LogTime = ShardingCoreHelper.ConvertDateTimeToLong(begin6)
                        });
                        begin6 = begin6.AddDays(1);
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
                    using (var tran = virtualDbContext.Database.BeginTransaction())
                    {
                        await virtualDbContext.AddRangeAsync(userMods);
                        await virtualDbContext.AddRangeAsync(userModInts);
                        await virtualDbContext.AddRangeAsync(userSalaries);
                        await virtualDbContext.AddRangeAsync(orders);
                        await virtualDbContext.AddRangeAsync(logDays);
                        await virtualDbContext.AddRangeAsync(logDayLongs);
                        await virtualDbContext.AddRangeAsync(logWeeks);
                        await virtualDbContext.AddRangeAsync(logWeekLongs);
                        await virtualDbContext.AddRangeAsync(logYears);
                        await virtualDbContext.AddRangeAsync(logMonthLongs);
                        await virtualDbContext.AddRangeAsync(logYearkLongs);
                        await virtualDbContext.AddRangeAsync(multiShardingOrders);

                        await virtualDbContext.SaveChangesAsync();
                        tran.Commit();
                    }
                }
            }
        }
    }
}