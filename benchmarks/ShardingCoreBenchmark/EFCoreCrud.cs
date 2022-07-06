using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Extensions;
using ShardingCore6x.NoShardingDbContexts;
using ShardingCore6x.ShardingDbContexts;
using System.Diagnostics;


namespace ShardingCore6x
{
    public class EFCoreCrud
    {
        private readonly DefaultDbContext _defaultDbContext;
        private readonly DefaultShardingDbContext _defaultShardingDbContext;
        private readonly ServiceCollection services;
        public EFCoreCrud()
        {
            services = new ServiceCollection();

            services.AddDbContext<DefaultDbContext>(o => o
                //.UseMySql("server=127.0.0.1;port=3306;database=db1;userid=root;password=L6yBtV6qNENrwBy7;", new MySqlServerVersion(new Version()))
                .UseSqlServer("Data Source=localhost;Initial Catalog=db1;Integrated Security=True;")
                , ServiceLifetime.Transient, ServiceLifetime.Transient);
            services.AddLogging();

            services.AddShardingDbContext<DefaultShardingDbContext>(ServiceLifetime.Transient, ServiceLifetime.Transient)
                .UseRouteConfig(o =>
                {
                    o.AddShardingTableRoute<OrderVirtualTableRoute>();
                })
                .UseConfig(op =>
                {
                    op.UseShardingQuery((conStr, builder) =>
                    {
                        builder.UseSqlServer(conStr);
                        //builder.UseMySql(conStr, new MySqlServerVersion(new Version()));
                    });
                    op.UseShardingTransaction((connection, builder) =>
                    {
                        builder.UseSqlServer(connection);
                        //builder.UseMySql(connection, new MySqlServerVersion(new Version()));
                    });
                    op.AddDefaultDataSource("ds0",
                        "Data Source=localhost;Initial Catalog=db2;Integrated Security=True;");

                    //op.AddDefaultDataSource("ds0", "server=127.0.0.1;port=3306;database=db2;userid=root;password=L6yBtV6qNENrwBy7;");

                }).AddShardingCore();

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
        //public void ActualConnectionStringManager()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var connectionString = _actualConnectionStringManager.GetConnectionString("ds0", false);
        //    }
        //}
        //[Benchmark]
        //public async Task CreateQueryable()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 3000000).ToString();
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next);
        //    }
        //}
        //[Benchmark]
        //public async Task DataSourceRouteRuleEngineFactory()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 3000000).ToString();
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next);
        //        _dataSourceRouteRuleEngineFactory.Route(queryable);
        //    }
        //}
        //[Benchmark]
        //public async Task TableRouteRuleEngineFactory()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 3000000).ToString();
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next);
        //        _tableRouteRuleEngineFactory.Route(queryable);
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingCreateStreamMergeContext()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1000000, 3000000).ToString();
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next);
        //        var firstOrDefaultAsync = _streamMergeContextFactory.Create(queryable, _defaultShardingDbContext);
        //    }
        //}
        //[Benchmark]
        //public async Task NoRouteParseCache()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1, 3000000).ToString();
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == next);
        //        _virtualTable.RouteTo(new ShardingTableRouteConfig(queryable: queryable));
        //    }
        //}

        //[Benchmark]
        //public async Task ShardingFirstOrDefaultAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var queryable = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == "1000");
        //        _virtualTable.RouteTo(new ShardingTableRouteConfig(queryable: queryable));
        //        var queryable1 = _defaultShardingDbContext.Set<Order>().Where(o => o.Id == "1000004");
        //        _virtualTable.RouteTo(new ShardingTableRouteConfig(queryable: queryable1));
        //    }
        //}
        //[Benchmark]
        //public async Task NoShardingFirstOrDefaultAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1, 7000000).ToString();
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == next);
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingFirstOrDefaultAsync()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var next = new Random().Next(1, 7000000).ToString();
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == next);
        //    }
        //}
        [Benchmark]
        public async Task NoShardingIndexFirstOrDefaultAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1000000, 7000000).ToString();
                var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == next);
            }
        }


        [Benchmark]
        public async Task ShardingIndexFirstOrDefaultAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1000000, 7000000).ToString();
                var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == next);
            }
        }
        [Benchmark]
        public async Task NoShardingNoIndexFirstOrDefaultAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1000000, 7000000);
                var firstOrDefaultAsync1 = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == next);
            }
        }


        [Benchmark]
        public async Task ShardingNoIndexFirstOrDefaultAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1000000, 7000000);
                var firstOrDefaultAsync1 = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == next);
            }
        }
        [Benchmark]
        public async Task NoShardingNoIndexCountAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1000000, 7000000);
                var firstOrDefaultAsync1 = await _defaultDbContext.Set<Order>().CountAsync(o => o.Amount == next);
            }
        }


        [Benchmark]
        public async Task ShardingNoIndexCountAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1000000, 7000000);
                var firstOrDefaultAsync1 = await _defaultShardingDbContext.Set<Order>().CountAsync(o => o.Amount == next);
            }
        }
        //[Benchmark]
        //public async Task NoShardingNoIndexFirstOrDefaultAsync0w()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == 0);
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingNoIndexFirstOrDefaultAsync0w()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == 0);
        //    }
        //}
        //[Benchmark]
        //public async Task NoShardingNoIndexFirstOrDefaultAsync99w()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == 999999);
        //    }
        //}
        //[Benchmark]
        //public async Task ShardingNoIndexFirstOrDefaultAsync99w()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Amount == 999999);
        //    }
        //}


        [Benchmark]
        public async Task NoShardingNoIndexLikeToListAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1, 7000000).ToString();
                var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().Where(o => o.Body.Contains(next)).ToListAsync();
            }
        }


        [Benchmark]
        public async Task ShardingNoIndexLikeToListAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1, 7000000).ToString();
                var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().Where(o => o.Body.Contains(next)).ToListAsync();
            }
        }
        [Benchmark]
        public async Task NoShardingNoIndexToListAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1, 7000000);
                var firstOrDefaultAsync = await _defaultDbContext.Set<Order>().Where(o => o.Amount == next).ToListAsync();
            }
        }


        [Benchmark]
        public async Task ShardingNoIndexToListAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var next = new Random().Next(1, 7000000);
                var firstOrDefaultAsync = await _defaultShardingDbContext.Set<Order>().Where(o => o.Amount == next).ToListAsync();
            }
        }
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
