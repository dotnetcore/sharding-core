using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;
using ShardingCoreBenchmark5x.NoShardingDbContexts;
using ShardingCoreBenchmark5x.ShardingDbContexts;

namespace ShardingCoreBenchmark5x
{
    public class EFCoreCrud
    {
        private readonly DefaultDbContext _defaultDbContext;
        private readonly DefaultShardingDbContext _defaultShardingDbContext;
        public EFCoreCrud()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DefaultDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=db1;Integrated Security=True;"), ServiceLifetime.Transient, ServiceLifetime.Transient);
            services.AddLogging();
            services.AddShardingDbContext<DefaultShardingDbContext>(ServiceLifetime.Transient, ServiceLifetime.Transient)
                .AddEntityConfig(o =>
                {
                    o.AddShardingTableRoute<OrderVirtualTableRoute>();
                })
                .AddConfig(op =>
                {
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr);
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection);
                    });
                    op.AddDefaultDataSource("ds0",
                        "Data Source=localhost;Initial Catalog=db2;Integrated Security=True;");

                }).ReplaceService<ITableEnsureManager,SqlServerTableEnsureManager>().EnsureConfig();

            var buildServiceProvider = services.BuildServiceProvider();
            buildServiceProvider.UseAutoShardingCreate();
            buildServiceProvider.UseAutoTryCompensateTable();
            ICollection<Order> orders = new LinkedList<Order>();

            using (var scope = buildServiceProvider.CreateScope())
            {
                var defaultShardingDbContext = scope.ServiceProvider.GetService<DefaultDbContext>();
                defaultShardingDbContext.Database.EnsureCreated();
                if (!defaultShardingDbContext.Set<Order>().Any())
                {
                    var begin = DateTime.Now.Date.AddDays(-8);
                    var now = DateTime.Now;
                    var current = begin;
                    int i = 0;
                    var x = new OrderStatusEnum[] { OrderStatusEnum.Failed, OrderStatusEnum.NotPay, OrderStatusEnum.Succeed };
                    while (current < now)
                    {
                        orders.Add(new Order()
                        {
                            Id = i.ToString(),
                            Amount = i,
                            Body = $"今天购买了的东西呀：{i}",
                            CreateTime = current,
                            Remark = $"这是我的备注哦备注哦备注哦：{i}",
                            Payer = Guid.NewGuid().ToString("n"),
                            OrderStatus = x[i % 3]
                        });
                        i++;
                        current = current.AddMilliseconds(100);
                    }
                    var sp = Stopwatch.StartNew();
                    defaultShardingDbContext.BulkInsert<Order>(orders.ToList());
                    sp.Stop();
                    Console.WriteLine($"批量插入订单数据:{orders.Count},用时:{sp.ElapsedMilliseconds}");
                }

            }
            using (var scope = buildServiceProvider.CreateScope())
            {
                var defaultShardingDbContext = scope.ServiceProvider.GetService<DefaultShardingDbContext>();
                if (!defaultShardingDbContext.Set<Order>().Any())
                {
                    var sp = Stopwatch.StartNew();
                    var bulkShardingEnumerable = defaultShardingDbContext.BulkShardingTableEnumerable(orders.ToList());
                    foreach (var keyValuePair in bulkShardingEnumerable)
                    {
                        keyValuePair.Key.BulkInsert(keyValuePair.Value.ToList());
                    }
                    sp.Stop();
                    Console.WriteLine($"批量插入订单数据:{orders.Count},用时:{sp.ElapsedMilliseconds}");
                }
            }
            _defaultDbContext = buildServiceProvider.GetService<DefaultDbContext>();
            _defaultShardingDbContext = buildServiceProvider.GetService<DefaultShardingDbContext>();
        }


        [Params(10)]
        public int N;

        [Benchmark]
        public async Task NoShardingFirstOrDefaultAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1, 3000000).ToString();
                var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == next);
            }
        }
        [Benchmark]
        public async Task ShardingFirstOrDefaultAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1, 3000000).ToString();
                var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == next);
            }
        }
        //[Benchmark]
        //public async Task NoShardingIndexFirstOrDefaultAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 7000000).ToString();
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == next);
        //    }
        //}


        //[Benchmark]
        //public async Task ShardingIndexFirstOrDefaultAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 7000000).ToString();
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == next);
        //    }
        //}
        //[Benchmark]
        //public async Task NoShardingNoIndexFirstOrDefaultAsync100w()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == 1000000);
        //    }
        //}


        //[Benchmark]
        //public async Task ShardingNoIndexFirstOrDefaultAsync100w()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == 1000000);
        //    }
        //}
        //[Benchmark]
        //public async Task NoShardingNoIndexCountAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().CountAsync(o => o.Amount == 3000000);
        //    }
        //}


        //[Benchmark]
        //public async Task ShardingNoIndexCountASYNC()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().CountAsync(o => o.Amount == 3000000);
        //    }
        //}
        //[Benchmark]
        //public async Task NoShardingNoIndexFirstOrDefaultAsync600w()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == 6000000);
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingNoIndexFirstOrDefaultAsync600w()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == 6000000);
        //    }
        //}


        //[Benchmark]
        //public async Task NoShardingNoIndexLikeToListAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1, 8000000).ToString();
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().Where(o => o.Body.Contains(next)).FirstOrDefaultAsync();
        //    }
        //}


        //[Benchmark]
        //public async Task ShardingNoIndexLikeToListAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1, 8000000).ToString();
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().Where(o => o.Body.Contains(next)).FirstOrDefaultAsync();
        //    }
        //}
        //[Benchmark]
        //public async Task NoShardingNoIndexToListAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(5000000, 7000000);
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().Where(o => o.Amount == next).ToListAsync();
        //    }
        //}


        //[Benchmark]
        //public async Task ShardingNoIndexToListAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(5000000, 7000000);
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().Where(o => o.Amount == next).ToListAsync();
        //    }
        //}
        //[Benchmark]
        //public void ShardingRouteFirstOrDefault()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 7000000).ToString();
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next);
        //        _virtualTable.RouteTo(new ShardingTableRouteConfig(queryable: queryable));
        //    }
        //}
        //private static readonly string[] aa = new string[] { "a", "b", "c", "d" };
        //[Benchmark]
        //public void ShardingCreateDbContextFirstOrDefault()
        //{
        //    for (int i = 0; i < N; i++)
        //    {

        //        var routeTail = _routeTailFactory.Create(aa[i % 4]);
        //        var dbContext = _defaultShardingDbContext.GetDbContext("ds0", true, routeTail);
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingWhereFirstOrDefaultAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 7000000).ToString();
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next).FirstOrDefaultAsync();
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingCreateStreamMergeContext()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 7000000).ToString();
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next);
        //        var firstOrDefaultAsync = _streamMergeContextFactory.Create(queryable, _defaultShardingDbContext);
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingCreateEnumerableQuery()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 7000000).ToString();
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next);
        //        new EnumerableQuery<Order>(queryable.Expression);
        //    }
        //}
        //private TResult GenericShardingDbContextMergeExecute<TResult>(IQueryable<Order> queryable)
        //{
        //    //{
        //    //    var queryEntityType = query.GetQueryEntityType();
        //    //var newStreamMergeEngineType = streamMergeEngineType.MakeGenericType(shardingDbContext.GetType(), queryEntityType);

        //    ////    {

        //    ////        //获取所有需要路由的表后缀
        //    ////        var startNew = Stopwatch.StartNew();
        //    ////        for (int i = 0; i < 10000; i++)
        //    ////        {
        //    ////            var streamEngine = ShardingCreatorHelper.CreateInstance(newStreamMergeEngineType, query, shardingDbContext);
        //    ////        }
        //    ////        startNew.Stop();
        //    ////        var x = startNew.ElapsedMilliseconds;
        //    ////    }
        //    //{

        //    //    //获取所有需要路由的表后缀
        //    //    var startNew1 = Stopwatch.StartNew();
        //    //    for (int i = 0; i < 10000; i++)
        //    //    {
        //    //        var streamEngine = Activator.CreateInstance(newStreamMergeEngineType, query, shardingDbContext);
        //    //    }
        //    //    startNew1.Stop();
        //    //    var x = startNew1.ElapsedMilliseconds;
        //    //}
        //    //    var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
        //    //    var streamEngineMethod = newStreamMergeEngineType.GetMethod(methodName);
        //    //    if (streamEngineMethod == null)
        //    //        throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
        //    //    var @params = async ? new object[] { cancellationToken } : new object[0];
        //    //}

        //    {
        //        var queryEntityType = queryable.Expression.GetQueryEntityType();
        //        var newStreamMergeEngineType = streamMergeEngineType.MakeGenericType(shardingDbContext.GetType(), queryEntityType);
        //        var streamEngine = Activator.CreateInstance(newStreamMergeEngineType, query, shardingDbContext);
        //        var methodName = async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult);
        //        var streamEngineMethod = newStreamMergeEngineType.GetMethod(methodName);
        //        if (streamEngineMethod == null)
        //            throw new ShardingCoreException($"cant found InMemoryAsyncStreamMergeEngine method [{methodName}]");
        //        var @params = async ? new object[] { cancellationToken } : new object[0];
        //        return (TResult)streamEngineMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(streamEngine, @params);
        //    }
        //}
        //[Benchmark]
        //public async Task NoShardingNoTrackingFirstOrDefaultAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1, 3000000).ToString();
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().AsNoTracking().FirstOrDefaultAsync(o => o.Id == next);
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingNoTrackingFirstOrDefaultAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1, 3000000).ToString();
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().AsNoTracking().FirstOrDefaultAsync(o => o.Id == next);
        //    }
        //}
        //[Benchmark]
        //public async Task NoShardingCountAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().CountAsync();
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingCountAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().CountAsync();
        //    }
        //}

    }
}
