using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using Sample.MySql.DbContexts;
using Sample.MySql.Domain.Entities;
using Sample.MySql.multi;
using Sample.MySql.Shardings;
using ShardingCore;
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.ShardingPageExtensions;
using ShardingCore.Extensions.ShardingQueryableExtensions;
using ShardingCore.Helpers;

namespace Sample.MySql.Controllers
{
    public class ssss
    {
        public string Id { get; set; }
        public int C { get; set; }
    }

    public class abc
    {
        public string id { get; set; }
        public string name { get; set; }
        public int count { get; set; }
    }

    public class ABC
    {
        private readonly DefaultShardingDbContext _defaultTableDbContext;

        public ABC(DefaultShardingDbContext defaultTableDbContext)
        {
            _defaultTableDbContext = defaultTableDbContext;
        }

        public IQueryable<SysTest> GetAll()
        {
            return _defaultTableDbContext.Set<SysTest>();
        }

        public virtual IQueryable<SysTest> Select => this.GetAll();
    }

    /// <summary>
    /// 接口限速
    /// </summary>
    public class RateLimitAttribute : Attribute
    {
        public RateLimitAttribute(int limitCount = 1)
        {
            Console.WriteLine("------------RateLimitAttribute----------");
        }
    }

    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IShardingRuntimeContext<DefaultShardingDbContext> _defaultShardingRuntimeContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly UnShardingDbContext _unShardingDbContext;
        private readonly DefaultShardingDbContext _defaultTableDbContext;
        private readonly IShardingRuntimeContext _shardingRuntimeContext;
        private readonly ABC _abc;

        public WeatherForecastController(
            IShardingRuntimeContext<DefaultShardingDbContext> defaultShardingRuntimeContext,
            IServiceProvider serviceProvider, UnShardingDbContext unShardingDbContext,
            DefaultShardingDbContext defaultTableDbContext, IShardingRuntimeContext shardingRuntimeContext)
        {
            _defaultShardingRuntimeContext = defaultShardingRuntimeContext;
            _serviceProvider = serviceProvider;
            _unShardingDbContext = unShardingDbContext;
            _defaultTableDbContext = defaultTableDbContext;
            _shardingRuntimeContext = shardingRuntimeContext;
            _abc = new ABC(_defaultTableDbContext);
        }

        // public (string id,string name___11) aaa()
        // {
        //    var sql= from u in _defaultTableDbContext.Set<SysTest>()
        //         where u.Id == "123"
        //         select (id:u.Id,name___11:u.UserId);
        //    return sql.FirstOrDefault();
        // }
        public IQueryable<SysTest> GetAll()
        {
            // var valueTuple = aaa();
            // valueTuple.


            var sql = from u in _defaultTableDbContext.Set<SysTest>()
                where u.Id == "123"
                select new
                {
                    Id = u.Id,
                    Name_____1111 = u.UserId
                };
            var sysTests = sql.Where(o => o.Name_____1111 == "456").ToList();


            var dbContextOptionsBuilder = new DbContextOptionsBuilder<DefaultShardingDbContext>();
            var dbContextOptionBuilderCreator = _shardingRuntimeContext.GetDbContextOptionBuilderCreator();
            dbContextOptionsBuilder.UseDefaultSharding<DefaultShardingDbContext>(_shardingRuntimeContext);
            using (var dbContext = new DefaultShardingDbContext(dbContextOptionsBuilder.Options))
            {
            }


            foreach (var dataSourceDbContext in _defaultTableDbContext.GetShardingExecutor().GetCurrentDbContexts())
            {
                foreach (var keyValuePair in dataSourceDbContext.Value.GetCurrentContexts())
                {
                }
            }

            ((IResettableService)_defaultTableDbContext).ResetState();

            var dataSourceRouteManager = _shardingRuntimeContext.GetDataSourceRouteManager();
            var entityMetadataManager = _shardingRuntimeContext.GetEntityMetadataManager();
            var allShardingEntities = entityMetadataManager.GetAllShardingEntities();
            foreach (var allShardingEntity in allShardingEntities)
            {
                var entityMetadata = entityMetadataManager.TryGet(allShardingEntity);
                if (entityMetadata != null && entityMetadata.IsShardingDataSource())
                {
                    var virtualDataSourceRoute = dataSourceRouteManager.GetRoute(entityMetadata.EntityType);
                    virtualDataSourceRoute.AddDataSourceName("ds9");
                }
            }
            DynamicShardingHelper.DynamicAppendDataSource(_shardingRuntimeContext,"ds9","链接字符串",true,true);
            
            
            
            
            
            
            
            
            

            // dataSourceRouteManager.GetRoute()
            var routeManager = _shardingRuntimeContext.GetTableRouteManager();
            var route = routeManager.GetRoute(typeof(SysTest));
            var tails = route.GetTails();
            // routeManager.GetRoute()

            DynamicShardingHelper.DynamicAppendDataSource(_shardingRuntimeContext,"ds9","链接字符串",true,true);
            //如果你已经添加好了的情况下并且没有生成对应的库和表想要生成表和库
            var dataSourceInitializer = _shardingRuntimeContext.GetDataSourceInitializer();
            dataSourceInitializer.InitConfigure("ds9", true, true);

            // _defaultTableDbContext.ReadWriteSeparationReadOnly();//读库
            // _defaultTableDbContext.ReadWriteSeparationWriteOnly();//写库
            var shardingTableCreator = _shardingRuntimeContext.GetShardingTableCreator();
            var tableRouteManager = routeManager;
            //系统的时间分片都会实现 ITailAppendable 如果不是系统的自定义的转成你自己的对象即可
            var tableRoute = tableRouteManager.GetRoute(typeof(SysUserMod));
            var virtualTableRoute = (ITailAppendable)tableRoute;

            //一定要先在路由里面添加尾巴
            virtualTableRoute.Append("20220921");
            shardingTableCreator.CreateTable<SysUserMod>("ds0", "20220921");
            return _defaultTableDbContext.Set<SysTest>();
        }

        [HttpGet]
        public async Task<IActionResult> Getxx()
        {
            var test = new Test();
            test.UtcTime = DateTime.Now;
            await _defaultTableDbContext.AddAsync(test);
            var saveChangesAsync = await _defaultTableDbContext.SaveChangesAsync();
            return Ok(saveChangesAsync);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var s = Guid.NewGuid().ToString();
            // var page =await _defaultTableDbContext.Set<SysUserLogByMonth>().Include().ThenInclude().Where(o=>o.Id==s).OrderByDescending(o=>o.Time).ToShardingPageAsync(1,2);
            // var virtualDataSource = _shardingRuntimeContext.GetVirtualDataSource();
            // virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource("2023", "xxxxxxxx", false));
            // var dataSourceRouteManager = _shardingRuntimeContext.GetDataSourceRouteManager();
            // var virtualDataSourceRoute = dataSourceRouteManager.GetRoute(typeof(SysUserMod));
            // virtualDataSourceRoute.AddDataSourceName("2023");
            // var dataSourceInitializer = _shardingRuntimeContext.GetDataSourceInitializer();
            // dataSourceInitializer.InitConfigure("2023",true,true);
            // using (var dbContextTransaction = _defaultTableDbContext.Database.BeginTransaction())
            // {
            //     
            // }

            var dateTime = new DateTime(2021, 1, 1);
            var x211 = await (from ut in _defaultTableDbContext.Set<SysTest>().UseUnionAllMerge()
                    join uu in _defaultTableDbContext.Set<SysUserLogByMonth>()
                        on ut.Id equals uu.Id
                    where uu.Time > dateTime
                    select new { a = ut, b = uu }).Select(o => new { x = o }).Select(o => new { x = o })
                .Select(o => new
                {
                    o.x.x.a.Id
                }).OrderBy(o => o.Id).ToListAsync();

            var x2 = await (from ut in _defaultTableDbContext.Set<SysTest>()
                    join uu in _defaultTableDbContext.Set<SysUserLogByMonth>()
                        on ut.Id equals uu.Id
                    select new { a = ut, b = uu }).Select(o => new { x = o }).Select(o => new { x = o })
                .Select(o => new
                {
                    o.x.x.a.Id
                }).OrderBy(o => o.Id).ToListAsync();
            Console.WriteLine("123");
            //OtherDbContext.CurrentId = "";
            // var myUsers0 = _otherDbContext.MyUsers.ToList();
            // OtherDbContext.CurrentId = "123";
            // var myUsers1 = _otherDbContext.MyUsers.ToList();
            // OtherDbContext.CurrentId = "456";
            // var myUsers2= _otherDbContext.MyUsers.ToList();

            // var sysUserModQueryable = _otherDbContext.MyUsers.Where(o => o.Id == "2");
            // var dbSetDiscoverExpressionVisitor = new DbSetDiscoverExpressionVisitor<MyUser>(_otherDbContext);
            // dbSetDiscoverExpressionVisitor.Visit(sysUserModQueryable.Expression);
            // var myUsers = dbSetDiscoverExpressionVisitor.DbSet;
            // Console.WriteLine("------------");
            // using (var tran = _defaultTableDbContext.Database.BeginTransaction())
            // {

            // SysUserMod? resultX123 = await _defaultTableDbContext.Set<SysUserMod>()
            //     .Where(o => o.Id == "2").FirstOrDefaultAsync();
            // _defaultTableDbContext.Update(resultX123);
            // _defaultTableDbContext.SaveChanges();

            Stopwatch sp = Stopwatch.StartNew();
            var sysUserMods = await _defaultTableDbContext.Set<SysUserMod>().ToListAsync();
            sp.Stop();
            Console.WriteLine(sp.ElapsedMilliseconds);
            sp.Restart();
            var sysUserMods11 = await _defaultTableDbContext.Set<SysUserMod>().AsNoTracking().ToListAsync();
            sp.Stop();
            Console.WriteLine(sp.ElapsedMilliseconds);
            sp.Restart();
            var sysUserMods22 = await _defaultTableDbContext.Set<SysUserMod>().ToListAsync();
            sp.Stop();
            Console.WriteLine(sp.ElapsedMilliseconds);

            var resultX1 = await _defaultTableDbContext.Set<SysUserMod>()
                .Where(o => o.Id == "2" || o.Id == "3").GroupBy(o => new { o.Id, o.Name })
                .Select(o => new
                {
                    id = o.Key.Id,
                    name = o.Key.Name,
                    count = o.Count()
                }).ToListAsync();
            var resultX12 = await _defaultTableDbContext.Set<SysUserMod>()
                .Where(o => o.Id == "2" || o.Id == "3").GroupBy(o => new { o.Id, o.Name })
                .Select(o => new abc
                {
                    id = o.Key.Id,
                    name = o.Key.Name,
                    count = o.Count()
                }).ToListAsync();

            var asyncEnumerator = _defaultTableDbContext.Set<SysTest>().AsAsyncEnumerable();
            await foreach (var sysTest in asyncEnumerator)
            {
                await Task.Delay(1000);
                Console.WriteLine("1");
            }

            var x = await (from ut in _defaultTableDbContext.Set<SysTest>()
                from uu in _defaultTableDbContext.Set<SysUserMod>()
                where ut.Id == uu.Id
                select ut).FirstOrDefaultAsync();
            var x1 = await (from ut in _defaultTableDbContext.Set<SysUserMod>()
                from uu in _defaultTableDbContext.Set<SysTest>()
                where ut.Id == uu.Id
                select ut).FirstOrDefaultAsync();
            // var firstOrDefault = _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw($"select * from {nameof(SysUserMod)}").FirstOrDefault();

            var sysUserMods1 = _defaultTableDbContext.Set<SysTest>().UseConnectionMode(1)
                .Select(o => new ssss() { Id = o.Id, C = _abc.Select.Count(x => x.Id == o.Id) }).ToList();
            var sysUserMods2 = _defaultTableDbContext.Set<SysTest>()
                .Select(o => new ssss() { Id = o.Id, C = GetAll().Count(x => x.Id == o.Id) }).ToList();
            var sysTests = GetAll();
            var sysUserMods3 = _defaultTableDbContext.Set<SysTest>()
                // .AsRoute(op=>
                // {
                //     op.TryCreateOrAddMustTail<SysTest>(new[] { "00" });
                // })
                .Select(o => new ssss() { Id = o.Id, C = sysTests.Count(x => x.Id == o.Id) }).ToList();
            var resultX = await _defaultTableDbContext.Set<SysUserMod>()
                .Where(o => o.Id == "2" || o.Id == "3").FirstOrDefaultAsync();
            var resultY = await _defaultTableDbContext.Set<SysUserMod>()
                .FirstOrDefaultAsync(o => o.Id == "2" || o.Id == "3");
            var shardingFirstOrDefaultAsyncxxx = await _defaultTableDbContext.Set<SysUserLogByMonth>()
                .Where(o => o.Time == DateTime.Now).ToListAsync();
            var result = await _defaultTableDbContext.Set<SysTest>().AnyAsync();
            var result22 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "2" && o.Name == "ds1")
                .ToListAsync();
            var result1 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "2" || o.Id == "3")
                .ToListAsync();
            var result2 = await _defaultTableDbContext.Set<SysUserLogByMonth>().Skip(1).Take(10).ToListAsync();
            var shardingFirstOrDefaultAsync = await _defaultTableDbContext.Set<SysUserLogByMonth>().ToListAsync();
            var shardingCountAsync = await _defaultTableDbContext.Set<SysUserMod>().CountAsync();
            var shardingCountAsyn2c = _defaultTableDbContext.Set<SysUserLogByMonth>().Count();


            // var dbConnection = _defaultTableDbContext.Database.GetDbConnection();
            // if (dbConnection.State != ConnectionState.Open)
            // {
            //     dbConnection.Open();
            // }
            // using (var dbCommand = dbConnection.CreateCommand())
            // {
            //     dbCommand.CommandText = "select * from systest";
            //     dbCommand.Transaction = _defaultTableDbContext.Database.CurrentTransaction?.GetDbTransaction();
            //     var dbDataReader = dbCommand.ExecuteReader();
            //     while (dbDataReader.Read())
            //     {
            //         Console.WriteLine(dbDataReader[0]);
            //     }
            // }
            // }

            return Ok(1);
        }

        [HttpGet]
        public async Task<IActionResult> Get1()
        {
            var resultX = await _defaultTableDbContext.Set<SysUserMod>().UseConnectionMode(1)
                .Where(o => o.Id == "2" || o.Id == "3").FirstOrDefaultAsync();
            Console.WriteLine(
                "-----------------------------------------------------------------------------------------------------");
            var resultY = await _defaultTableDbContext.Set<SysUserMod>()
                .FirstOrDefaultAsync(o => o.Id == "2" || o.Id == "3");
            Console.WriteLine(
                "-----------------------------------------------------------------------------------------------------");
            var result = await _defaultTableDbContext.Set<SysTest>().AnyAsync();
            Console.WriteLine(
                "-----------------------------------------------------------------------------------------------------");
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get2()
        {
            var dateTime = new DateTime(2021, 1, 1);
            var sql = from a in _defaultTableDbContext.Set<SysUserLogByMonth>().Where(o => o.Time == dateTime)
                join b in _defaultTableDbContext.Set<SysTest>()
                    on a.Id equals b.Id into t1
                from aa1 in t1.DefaultIfEmpty()
                select new
                {
                    ID = a.Id
                };
            var listAsync = await sql.ToListAsync();
            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get3()
        {
            var sysUserMods = await _defaultTableDbContext.Set<SysUserMod>().FirstOrDefaultAsync();
            _defaultTableDbContext.SysUserMod.Remove(sysUserMods);
            _defaultTableDbContext.SaveChanges();
            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get4()
        {
            var sql = from a in _defaultTableDbContext.Set<TestMod>()
                join b in _defaultTableDbContext.Set<TestModItem>()
                    on a.Id equals b.MainId
                select a;
            var xx = await sql.ToListAsync();
            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get6()
        {
            var sysUserMod = await _defaultTableDbContext.Set<SysUserMod>().FirstOrDefaultAsync();
            sysUserMod.Age = new Random().Next(1, 999);
            _defaultTableDbContext.Update(sysUserMod);
            _defaultTableDbContext.SaveChanges();
            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get7()
        {
            var sysUserMod = await _defaultTableDbContext.Set<SysUserMod>().FindAsync("101");
            sysUserMod.Age = new Random().Next(1, 999);
            _defaultTableDbContext.Update(sysUserMod);
            _defaultTableDbContext.SaveChanges();
            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get8()
        {
            var sysUserMod = await _defaultTableDbContext.Set<SysUserMod>().AsNoTracking().FirstOrDefaultAsync();
            _defaultTableDbContext.Update(sysUserMod);
            await _defaultTableDbContext.SaveChangesAsync();
            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get9()
        {
            var sysUserMod = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Name == "11231")
                .AllAsync(o => o.Id == "1123");

            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get10()
        {
            var shardingDbContextExecutor = _defaultTableDbContext.GetShardingExecutor();
            var dataSourceDbContexts = shardingDbContextExecutor.GetCurrentDbContexts();
            var sourceDbContexts = dataSourceDbContexts.Values;
            var dataSourceDbContext = sourceDbContexts.FirstOrDefault();
            var dbConnection = dataSourceDbContext.GetCurrentContexts().FirstOrDefault().Value.Database
                .GetDbConnection();


            var sysUserMod1 = await _defaultTableDbContext.Set<SysTest>().Where(o => o.UserId == "11231")
                .AllAsync(o => o.Id == "1123" && string.Compare(o.UserId, "123") > 0);
            var sysUserMod2 = await _defaultTableDbContext.Set<SysTest>().AllAsync(o => o.Id == "1123");
            var dateTime = new DateTime(2020, 1, 1);
            var sysUserMod3 = await _defaultTableDbContext.Set<SysUserLogByMonth>().Where(o => o.Time > dateTime)
                .ToListAsync();

            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get11()
        {
            using (var tran = await _defaultTableDbContext.Database.BeginTransactionAsync())
            {
                var newGuid = Guid.NewGuid().ToString("n");
                await _defaultTableDbContext.Set<SysTest>().Where(o => o.Id == "11")
                    .ExecuteUpdateAsync(o => o.SetProperty(x => x.UserId, x => newGuid));
                throw new Exception("123");
                await tran.CommitAsync();
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get12()
        {
            var sysTests = from ut in _defaultTableDbContext.Set<SysTest>()
                where _defaultTableDbContext.Set<SysUserLogByMonth>().Any(x => x.Id == ut.Id)
                select ut;
            var tests = sysTests.ToList();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get13()
        {
            var list = await _defaultTableDbContext.Set<SysUserLogByMonth>()
                .GroupBy(o => o.Time)
                .Select(o => new
                {
                    o.Key
                }).ToListAsync();
            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> Get14()
        {
            var sysUserLogByMonths = new List<SysUserLogByMonth>();
            for (int i = 0; i < 100; i++)
            {
                var sysUserLogByMonth = new SysUserLogByMonth();
                sysUserLogByMonth.Id = Guid.NewGuid().ToString("n");
                sysUserLogByMonth.Time = DateTime.Now;
                sysUserLogByMonths.Add(sysUserLogByMonth);
            }

            await _defaultTableDbContext.AddRangeAsync(sysUserLogByMonths);
            await _defaultTableDbContext.SaveChangesAsync();
            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> Get15()
        {
            var sysUserLogByMonths = new List<SysUserLogByMonth>();
            for (int i = 0; i < 100; i++)
            {
                var sysUserLogByMonth = new SysUserLogByMonth();
                sysUserLogByMonth.Id = Guid.NewGuid().ToString("n");
                sysUserLogByMonth.Time = DateTime.Now;
                sysUserLogByMonths.Add(sysUserLogByMonth);
            }

            await _unShardingDbContext.AddRangeAsync(sysUserLogByMonths);
            await _unShardingDbContext.SaveChangesAsync();
            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> Get16()
        {
            _defaultTableDbContext.ReadWriteSeparationWriteOnly();

            var sysUserMod = await _defaultTableDbContext.Set<SysUserMod>().FirstOrDefaultAsync();
            sysUserMod.Age = new Random().Next(1, 999);
            await _defaultTableDbContext.SaveChangesAsync();
            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get17()
        {
            var sysUserMods = await _defaultTableDbContext.SysUserMod.AsNoTracking()
                .Select(o => new SysUserMod()
                {
                    Name = o.Name,
                    Age = o.Age
                }).OrderBy(x => EF.Property<object>(x, "Name")).ToListAsync();

            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok(sysUserMods);
        }

        [HttpGet]
        public async Task<IActionResult> Get18()
        {
            var sysUserMods = await _defaultTableDbContext.SysUserMod.AsNoTracking().GroupBy(o => o.Age)
                .Select(o => new
                {
                    Age = o.Key
                })
                .CountAsync();

            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok(sysUserMods);
        }

        [HttpGet]
        public async Task<IActionResult> Get20()
        {
            var dateTime = new DateTime(2022, 1, 1);
            var queryable = _defaultTableDbContext.Set<SysUserLogByMonth>().Where(o => o.Time > dateTime)
                .Select(o => o.Id);
            var sysUserMods = await _defaultTableDbContext.SysUserMod.AsNoTracking()
                .Where(o => queryable.Contains(o.Id)).ToListAsync();

            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok(sysUserMods);
        }

        [HttpGet]
        public async Task<IActionResult> Get211()
        {
            var sysUserMod = new SysUserMod();
            sysUserMod.Id = "xxxx111x";
            sysUserMod.Age = 1;
            sysUserMod.Name = "ds0";
            await _defaultTableDbContext.AddAsync(sysUserMod);
            await _defaultTableDbContext.SaveChangesAsync();
            // var sysUserMods1 = await _defaultTableDbContext.Set<SysUserMod>().FromSqlRaw("select * from SysUserMod where id='2'").ToListAsync();
            // var sysUserMods2 = await _defaultTableDbContext.Set<SysTest>().FromSqlRaw("select * from SysTest where id='2'").ToListAsync();
            return Ok();
        }

        // public void batachSave()
        // {
        //     var objects = new List<List<object>>();
        //     foreach (List<object> o in objects)
        //     {
        //         Task.Run(()=>
        //         {
        //             using (var serviceScope = _serviceProvider.CreateScope())
        //             {
        //                 var asyncBatchService = serviceScope.ServiceProvider.GetService<AsyncBatchService>();
        //                 asyncBatchService.SaveInOtherThread(o);
        //             }
        //         });
        //     }
        // }

        public void get11()
        {
            var mySqlConnection = new MySqlConnection("链接字符串");
            var dbContextOptions1 = new DbContextOptionsBuilder<UnShardingDbContext>().UseMySql(mySqlConnection,
                new MySqlServerVersion(new Version()),
                b => { b.EnableRetryOnFailure(10, TimeSpan.FromSeconds(10), null); }).Options;
            var dbContextOptions2 = new DbContextOptionsBuilder<UnShardingDbContext>()
                .UseMySql(mySqlConnection, new MySqlServerVersion(new Version())).Options;
            var unShardingDbContext1 = new UnShardingDbContext(dbContextOptions1); //映射到202301模型
            var unShardingDbContext2 = new UnShardingDbContext(dbContextOptions2); //映射到202302模型
            // unShardingDbContext2.Database.CreateExecutionStrategy()
            var dbContextTransaction = unShardingDbContext1.Database.BeginTransaction();
            unShardingDbContext2.Database.UseTransaction(dbContextTransaction.GetDbTransaction());
            //unShardingDbContext1.add()
            //unShardingDbContext2.add()
            unShardingDbContext1.SaveChanges();
            unShardingDbContext2.SaveChanges();
            dbContextTransaction.Commit();
        }

        [HttpGet]
        public async Task<IActionResult> get131()
        {
            var list = new List<string>();
            var idList = Enumerable.Range(1, 50000).Select(o => o.ToString()).ToList();
            var sysUserMods = _defaultTableDbContext.Set<SysUserMod>()
                .Where(o => idList.Contains(o.Id)).ToList();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> get1312()
        {
            var listAsync = await _defaultTableDbContext.Database.SqlQuery<MyClass>($"select * from groupentity_00")
                .ToListAsync();
            var list1 = _defaultTableDbContext.Set<GroupEntity>()
                .UseConnectionMode(1, ConnectionModeEnum.CONNECTION_STRICTLY)
                // .Where(o=>o.City=="郑州市")
                .GroupBy(o => new
                {
                    o.City,
                    o.Area
                })
                .Select(o => new
                {
                    o.Key.City,
                    o.Key.Area,
                    UserSum = o.Count()
                }).ToList();

            return Ok(list1);
        }

        public class MyClass
        {
            public string Id { get; set; }
            public string City { get; set; }
            public string Area { get; set; }
        }
    }
}