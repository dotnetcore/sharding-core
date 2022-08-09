using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.ShardingPageExtensions;
using ShardingCore.Extensions.ShardingQueryableExtensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;
using ShardingCore.Sharding.StreamMergeEngines;
using ShardingCore.TableCreator;
using ShardingCore.Test3x.Domain.Entities;
using ShardingCore.Utils;
using Xunit;

namespace ShardingCore.Test3x
{

    public class ShardingTest
    {
        private readonly ShardingDefaultDbContext _virtualDbContext;
        private readonly IShardingRouteManager _shardingRouteManager;
        private readonly ActualConnectionStringManager _connectionStringManager;
        private readonly IConfiguration _configuration;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly ITableRouteManager _tableRouteManager;
        private readonly IShardingTableCreator _shardingTableCreator;
        private readonly IShardingReadWriteManager _shardingReadWriteManager;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IReadWriteConnectorFactory _readWriteConnectorFactory;
        private readonly IShardingConnectionStringResolver _shardingConnectionStringResolver;
        private readonly IShardingComparer _shardingComparer;

        public ShardingTest(ShardingDefaultDbContext virtualDbContext, IShardingRuntimeContext shardingRuntimeContext, IConfiguration configuration)
        {
            _virtualDbContext = virtualDbContext;
            _shardingRouteManager = shardingRuntimeContext.GetShardingRouteManager();
            _shardingReadWriteManager = shardingRuntimeContext.GetShardingReadWriteManager();
            _virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            _connectionStringManager = new ActualConnectionStringManager(_shardingReadWriteManager,_virtualDataSource);
            _configuration = configuration;
            _entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();
            _tableRouteManager = shardingRuntimeContext.GetTableRouteManager();
            _shardingTableCreator = shardingRuntimeContext.GetShardingTableCreator();
            _routeTailFactory = shardingRuntimeContext.GetRouteTailFactory();
            _shardingComparer = shardingRuntimeContext.GetShardingComparer();
            _readWriteConnectorFactory = shardingRuntimeContext.GetReadWriteConnectorFactory();
            var readWriteConnectors = _virtualDataSource.ConfigurationParams.ReadWriteNodeSeparationConfigs.Select(o => _readWriteConnectorFactory.CreateConnector(_virtualDataSource.ConfigurationParams.ReadStrategy.GetValueOrDefault(), o.Key, o.Value));
            _shardingConnectionStringResolver = new ReadWriteShardingConnectionStringResolver(readWriteConnectors, _virtualDataSource.ConfigurationParams.ReadStrategy.GetValueOrDefault(),_readWriteConnectorFactory);
        }
        // [Fact]
        // public void RouteParseCompileCacheTest()
        // {
        //     var expressionEqualityComparer = new RouteParseExpressionEqualityComparer();
        //     var virtualTable = _virtualTableManager.GetVirtualTable<SysUserSalary>();
        //     var virtualTableRoute = (AbstractShardingOperatorVirtualTableRoute<SysUserSalary, int>)virtualTable.GetVirtualRoute();
        //     var xxxx = "202102";
        //     var queryable1 = _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth >= 202102);
        //     var routeParseExpression1 = ShardingUtil.GetRouteParseExpression(queryable1, virtualTableRoute.EntityMetadata,
        //         (i, op, propertyName) => virtualTableRoute.GetRouteFilter(i, op, propertyName), true);
        //     var queryable2 = _virtualDbContext.Set<SysUserSalary>().Where(ox => ox.DateOfMonth >= 202102);
        //     var routeParseExpression2 = ShardingUtil.GetRouteParseExpression(queryable2, virtualTableRoute.EntityMetadata,
        //         (i, op, propertyName) => virtualTableRoute.GetRouteFilter(i, op, propertyName), true);
        //     var xxxx1 = 202102;
        //     var queryable3 = _virtualDbContext.Set<SysUserSalary>().Where(ox => ox.DateOfMonth >= xxxx1);
        //     var routeParseExpression3 = ShardingUtil.GetRouteParseExpression(queryable3, virtualTableRoute.EntityMetadata,
        //         (i, op, propertyName) => virtualTableRoute.GetRouteFilter(i, op, propertyName), true);
        //     var queryable4 = _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth >= 202101);
        //     var routeParseExpression4 = ShardingUtil.GetRouteParseExpression(queryable4, virtualTableRoute.EntityMetadata,
        //         (i, op, propertyName) => virtualTableRoute.GetRouteFilter(i, op, propertyName), true);
        //     var queryable5 = _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth > 202101);
        //     var routeParseExpression5 = ShardingUtil.GetRouteParseExpression(queryable5, virtualTableRoute.EntityMetadata,
        //         (i, op, propertyName) => virtualTableRoute.GetRouteFilter(i, op, propertyName), true);
        //     var queryable6 = _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth == 202101);
        //     var routeParseExpression6 = ShardingUtil.GetRouteParseExpression(queryable6, virtualTableRoute.EntityMetadata,
        //         (i, op, propertyName) => virtualTableRoute.GetRouteFilter(i, op, propertyName), true);
        //     var queryable7 = _virtualDbContext.Set<SysUserSalary>().Where(o => 202101 <= o.DateOfMonth);
        //     var routeParseExpression7 = ShardingUtil.GetRouteParseExpression(queryable7, virtualTableRoute.EntityMetadata,
        //         (i, op, propertyName) => virtualTableRoute.GetRouteFilter(i, op, propertyName), true);
        //     var queryable8 = _virtualDbContext.Set<SysUserSalary>().Where(o => 202101 == o.DateOfMonth);
        //     var routeParseExpression8 = ShardingUtil.GetRouteParseExpression(queryable8, virtualTableRoute.EntityMetadata,
        //         (i, op, propertyName) => virtualTableRoute.GetRouteFilter(i, op, propertyName), true);
        //     Assert.Equal(expressionEqualityComparer.GetHashCode(routeParseExpression1), expressionEqualityComparer.GetHashCode(routeParseExpression2));
        //     Assert.Equal(expressionEqualityComparer.GetHashCode(routeParseExpression1), expressionEqualityComparer.GetHashCode(routeParseExpression3));
        //     Assert.NotEqual(expressionEqualityComparer.GetHashCode(routeParseExpression1), expressionEqualityComparer.GetHashCode(routeParseExpression4));
        //     Assert.Equal(expressionEqualityComparer.GetHashCode(routeParseExpression4), expressionEqualityComparer.GetHashCode(routeParseExpression5));
        //     Assert.NotEqual(expressionEqualityComparer.GetHashCode(routeParseExpression5), expressionEqualityComparer.GetHashCode(routeParseExpression6));
        //     Assert.Equal(expressionEqualityComparer.GetHashCode(routeParseExpression4), expressionEqualityComparer.GetHashCode(routeParseExpression7));
        //     Assert.Equal(expressionEqualityComparer.GetHashCode(routeParseExpression6), expressionEqualityComparer.GetHashCode(routeParseExpression8));
        //
        // }

        [Fact]
        public async Task GenericTest()
        {
            var a = new DefaultPhysicDataSource("aaa", "aaa", true);
            var b = new DefaultPhysicDataSource("aaa", "aaa1", false);
            Assert.Equal(a, b);
            var x = new EntityMetadata(typeof(LogDay));
            var y = new EntityMetadata(typeof(LogDay));
            Assert.Equal(x, y);
            var dateTime = new DateTime(2021, 1, 1);
            var logDays = Enumerable.Range(0, 100).Select(o => new LogDay() { Id = Guid.NewGuid(), LogLevel = "info", LogBody = o.ToString(), LogTime = dateTime.AddDays(o) }).ToList();

            var bulkShardingTableEnumerable = _virtualDbContext.BulkShardingTableEnumerable(logDays);

            Assert.Equal(100, bulkShardingTableEnumerable.Count);
            var bulkShardingEnumerable = _virtualDbContext.BulkShardingEnumerable(logDays);
            Assert.Equal(1, bulkShardingEnumerable.Count);
            foreach (var (key, value) in bulkShardingEnumerable)
            {
                Assert.Equal(100, value.Count);
            }

            _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "300").ShardingPrint();
            var contains = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "300").Select(o => o.Id).ContainsAsync("300");
            Assert.True(contains);

            try
            {
                var x1 = _virtualDataSource.GetPhysicDataSource("abc");
            }
            catch (Exception e)
            {
                Assert.Equal(typeof(ShardingCoreNotFoundException), e.GetType());
            }

            var queryable = new List<string>().Select(o => new SequenceClass { Id = "123", T = o }).AsQueryable();
            var sourceType = queryable.GetType().GetSequenceType();
            Assert.Equal(typeof(SequenceClass), sourceType);
            try
            {
                _shardingTableCreator.CreateTable<Order>("A", "202105");
            }
            catch (Exception e)
            {
                Assert.Equal(typeof(ShardingCoreException), e.GetType());
            }

            var orderMetadata = _entityMetadataManager.TryGet<Order>();
            Assert.NotNull(orderMetadata);
            var isKey1 = orderMetadata.ShardingDataSourceFieldIsKey();
            Assert.False(isKey1);
            var isKey2 = orderMetadata.ShardingTableFieldIsKey();
            Assert.False(isKey2);
            var userModMetadata = _entityMetadataManager.TryGet<SysUserMod>();
            Assert.NotNull(userModMetadata);
            var isKey3 = userModMetadata.ShardingDataSourceFieldIsKey();
            Assert.False(isKey3);
            var isKey4 = userModMetadata.ShardingTableFieldIsKey();
            Assert.True(isKey4);

            await _virtualDbContext.AddRangeAsync(logDays);
            var bulkShardingExpression = _virtualDbContext.BulkShardingExpression<ShardingDefaultDbContext, Order>(o => new[] { "A", "B" }.Contains(o.Area));
            Assert.Equal(2, bulkShardingExpression.Count);
            Assert.True(bulkShardingExpression.ContainsKey("A"));
            Assert.True(bulkShardingExpression.ContainsKey("B"));

            var bulkShardingTableExpression = _virtualDbContext.BulkShardingTableExpression<ShardingDefaultDbContext, SysUserMod>(o => o.Id == Guid.NewGuid().ToString());

            Assert.Equal(1, bulkShardingTableExpression.Count());

            var noShardingExpression = _virtualDbContext.BulkShardingExpression<ShardingDefaultDbContext, LogNoSharding>(o => o.Id == "123");
            Assert.Equal(1, noShardingExpression.Count());

            var isShardingDbContext = _virtualDbContext.IsShardingDbContext();
            Assert.True(isShardingDbContext);
            var isShardingTableDbContext = _virtualDbContext.IsShardingTableDbContext();
            Assert.True(isShardingTableDbContext);
            var shardingDbContext = _virtualDbContext.GetType().IsShardingDbContext();
            Assert.True(shardingDbContext);
            var shardingTableDbContext = _virtualDbContext.GetType().IsShardingTableDbContext();
            Assert.True(shardingTableDbContext);
            var emptyTailIdentity = new SingleQueryRouteTail(string.Empty).GetRouteTailIdentity();
            var aTailIdentity = new SingleQueryRouteTail("a").GetRouteTailIdentity();
            var bTailIdentity = new SingleQueryRouteTail("b").GetRouteTailIdentity();
            var dics = new SortedDictionary<string, string>(new NoShardingFirstComparer());
            var dicTails = new List<string>() { emptyTailIdentity, aTailIdentity, bTailIdentity };
            for (int i = 0; i < 10; i++)
            {
                dics.Clear();
                var reOrderList = dicTails.OrderBy(o => Guid.NewGuid()).ToList();
                foreach (var tail in reOrderList)
                {
                    dics.Add(tail, null);
                }
                Assert.Equal($"{emptyTailIdentity},{aTailIdentity},{bTailIdentity}", string.Join(",", dics.Keys));
            }

            var x1x1 = new ParallelTableGroupNode(new HashSet<ParallelTableComparerType>()
                { new ParallelTableComparerType(typeof(SysUserMod)), new ParallelTableComparerType(typeof(SysUserSalary)) });
            var x2x2 = new ParallelTableGroupNode(new HashSet<ParallelTableComparerType>()
                {  new ParallelTableComparerType(typeof(SysUserSalary)),new ParallelTableComparerType(typeof(SysUserMod)), });
            Assert.Equal(x1x1, x2x2);
            Assert.Equal(x1x1.GetHashCode(), x2x2.GetHashCode());
            var succeedAddConnectionString = _shardingConnectionStringResolver.AddConnectionString("X", "Data Source=localhost;Initial Catalog=ShardingCoreDBC;Integrated Security=True;",null);
            Assert.True(succeedAddConnectionString);
            var connectionString = _shardingConnectionStringResolver.GetConnectionString("X", null);
            Assert.Equal("Data Source=localhost;Initial Catalog=ShardingCoreDBC;Integrated Security=True;", connectionString);
        }

        public class SequenceClass
        {
            public string Id { get; set; }
            public string T { get; set; }
        }

        [Fact]
        public async Task TestMultiShardingProperty()
        {

            var multiOrder = await _virtualDbContext.Set<MultiShardingOrder>().Where(o => o.Id == 232398109278351360).FirstOrDefaultAsync();
            Assert.NotNull(multiOrder);
            var allMultiOrders = await _virtualDbContext.Set<MultiShardingOrder>().ToListAsync();
            Assert.Equal(8, allMultiOrders.Count);
            var longs = new[] { 232398109278351360, 255197859283087360 };
            var multiOrders = await _virtualDbContext.Set<MultiShardingOrder>().Where(o => longs.Contains(o.Id)).ToListAsync();
            Assert.Equal(2, multiOrders.Count);
            var multinNotOrders = await _virtualDbContext.Set<MultiShardingOrder>().Where(o => !longs.Contains(o.Id)).ToListAsync();
            Assert.Equal(6, multinNotOrders.Count);
            var dateTime = new DateTime(2021, 11, 1);
            var multiOrder404 = await _virtualDbContext.Set<MultiShardingOrder>().Where(o => o.Id == 250345338962063360 && o.CreateTime < dateTime).FirstOrDefaultAsync();
            Assert.Null(multiOrder404);
        }
        [Fact]
        public void TestEntityMetadataManager()
        {
            var objMetadata0 = _entityMetadataManager.TryGet(typeof(object));
            Assert.Null(objMetadata0);
            var objMetadata1 = _entityMetadataManager.TryGet<object>();
            Assert.Null(objMetadata1);

            var objMetadata2 = _entityMetadataManager.TryGet(typeof(SysUserMod));
            Assert.NotNull(objMetadata2);
            var objMetadata3 = _entityMetadataManager.TryGet<SysUserMod>();
            Assert.NotNull(objMetadata3);
            var sysUserModIsShardingTable0 = _entityMetadataManager.IsShardingTable(typeof(SysUserMod));
            Assert.True(sysUserModIsShardingTable0);
            var sysUserModIsShardingTable1 = _entityMetadataManager.IsShardingTable<SysUserMod>();
            Assert.True(sysUserModIsShardingTable1);
            var sysUserModIsShardingDataSource0 = _entityMetadataManager.IsShardingDataSource(typeof(SysUserMod));
            Assert.False(sysUserModIsShardingDataSource0);
            var sysUserModIsShardingDataSource1 = _entityMetadataManager.IsShardingDataSource<SysUserMod>();
            Assert.False(sysUserModIsShardingDataSource1);
        }
        [Fact]
        public void TestShardingComparer()
        {
            var x = new Guid("7CDE28F8-D548-B96D-1C61-39FFE37AE492");
            var y = new Guid("3425D899-291D-921B-DDE4-49FFE37AE493");
            //asc y<x c# compare guid
            var compare0 = x.CompareTo(y);
            Assert.True(compare0 > 0);
            //asc x<y db compare  uniqueidentifier
            var compare1 = _shardingComparer.Compare(x, y, true);
            Assert.True(compare1 < 0);
        }
        [Fact]
        public void TestConnectionStringManager()
        {
            _virtualDbContext.ReadWriteSeparationReadOnly();
            var connectionString1 = _connectionStringManager.GetConnectionString(_virtualDataSource.DefaultDataSourceName, true);
            Assert.Equal(connectionString1, "Data Source=localhost;Initial Catalog=ShardingCoreDBA;Integrated Security=True;");
            _virtualDbContext.ReadWriteSeparationWriteOnly();
        }
        //[Fact]
        //public async Task Route_TEST()
        //{
        //    var queryable1 = _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id=="339");
        //    var routeResults1 = _tableRouteRuleEngineFactory.Route(queryable1);
        //    Assert.Equal(1,routeResults1.Count());
        //    Assert.Equal(1,routeResults1.FirstOrDefault().ReplaceTables.Count());
        //    Assert.Equal("0",routeResults1.FirstOrDefault().ReplaceTables.FirstOrDefault().Tail);
        //    Assert.Equal(nameof(SysUserMod),routeResults1.FirstOrDefault().ReplaceTables.FirstOrDefault().OriginalName);
        //    var ids = new[] {"339", "124","142"};
        //    var queryable2= _virtualDbContext.Set<SysUserMod>().Where(o=>ids.Contains(o.Id));
        //    var routeResult2s = _tableRouteRuleEngineFactory.Route(queryable2);
        //    Assert.Equal(2,routeResult2s.Count());
        //    Assert.Equal(1,routeResult2s.FirstOrDefault().ReplaceTables.Count());
        //    Assert.Equal(2,routeResult2s.SelectMany(o=>o.ReplaceTables).Count());
        //    Assert.Equal(true,routeResult2s.SelectMany(o=>o.ReplaceTables).All(o=>new[]{"0","1"}.Contains(o.Tail)));
        //}
        [Fact]
        public async Task ToList_All_Route_Test()
        {
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<SysUserMod>("00");

                var mod00s = await _virtualDbContext.Set<SysUserMod>().ToListAsync();
                Assert.Equal(333, mod00s.Count);
            }
            var mods = await _virtualDbContext.Set<SysUserMod>().ToListAsync();
            Assert.Equal(1000, mods.Count);

            var modOrders1 = await _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToListAsync();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }


            var modOrders2 = await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToListAsync();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }
        }
        [Fact]
        public async Task ToList_All_Test()
        {

            var mods = await _virtualDbContext.Set<SysUserMod>().ToListAsync();
            Assert.Equal(1000, mods.Count);

            var modOrders1 = await _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToListAsync();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }

            var modOrders2 = await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToListAsync();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }



            var pageResult = await _virtualDbContext.Set<SysUserMod>().Skip(10).Take(10).OrderByDescending(o => o.Age).ToListAsync();
            Assert.Equal(10, pageResult.Count);
            int pageDescAge = 990;
            foreach (var sysUserMod in pageResult)
            {
                Assert.Equal(pageDescAge, sysUserMod.Age);
                pageDescAge--;
            }
            var skip10First = await _virtualDbContext.Set<SysUserMod>().Skip(10).OrderByDescending(o => o.Age).FirstOrDefaultAsync();
            Assert.Equal(skip10First, pageResult[0]);
        }

        [Fact]
        public async Task ToList_Join_Test()
        {
            var list111 = await (from u in _virtualDbContext.Set<SysUserMod>()
                                 join salary in _virtualDbContext.Set<SysUserSalary>()
                                     on u.Id equals salary.UserId
                                 select new
                                 {
                                     u.Id,
                                     u.Age,
                                     Salary = salary.Salary,
                                     DateOfMonth = salary.DateOfMonth,
                                     Name = u.Name
                                 }).CountAsync();
            var list = await (from u in _virtualDbContext.Set<SysUserMod>()
                              join salary in _virtualDbContext.Set<SysUserSalary>()
                                  on u.Id equals salary.UserId
                              select new
                              {
                                  u.Id,
                                  u.Age,
                                  Salary = salary.Salary,
                                  DateOfMonth = salary.DateOfMonth,
                                  Name = u.Name
                              }).ToListAsync();

            var list2 = list.OrderBy(o => o.Age).Select(o => o.Age).Distinct().ToList();
            Assert.Equal(24000, list.Count());
            Assert.Equal(24, list.Count(o => o.Name == "name_200"));


            var queryable = (from u in _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "300")
                             join salary in _virtualDbContext.Set<SysUserSalary>()
                                 on u.Id equals salary.UserId
                             select new
                             {
                                 Salary = salary.Salary,
                                 DateOfMonth = salary.DateOfMonth,
                                 Name = u.Name
                             });
            var list1 = await queryable.ToListAsync();
            Assert.Equal(24, list1.Count());
            Assert.DoesNotContain(list1, o => o.Name != "name_300");

            var queryable1 = (from u in _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "300")
                              join salary in _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth == 202005)
                                  on u.Id equals salary.UserId
                              select new
                              {
                                  Salary = salary.Salary,
                                  DateOfMonth = salary.DateOfMonth,
                                  Name = u.Name
                              });
            var list3 = await queryable1.ToListAsync();
            Assert.Equal(1, list3.Count());
            Assert.Contains(list3, o => o.Name == "name_300");
            var firstOrDefaultAsync = await queryable1.OrderBy(o => o.DateOfMonth).FirstOrDefaultAsync();
            Assert.NotNull(firstOrDefaultAsync);
            var firstOrDefault = queryable1.OrderBy(o => o.DateOfMonth).FirstOrDefault();
            Assert.NotNull(firstOrDefault);
            Assert.Equal(firstOrDefaultAsync, firstOrDefault);
        }

        [Fact]
        public async Task ToList_OrderBy_Asc_Desc_Test()
        {
            var modascs = await _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToListAsync();
            Assert.Equal(1000, modascs.Count);
            var i = 1;
            foreach (var age in modascs)
            {
                Assert.Equal(i, age.Age);
                i++;
            }

            var moddescs = await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToListAsync();
            Assert.Equal(1000, moddescs.Count);
            var j = 1000;
            foreach (var age in moddescs)
            {
                Assert.Equal(j, age.Age);
                j--;
            }
        }

        [Fact]
        public async Task ToList_Id_In_Test()
        {
            var ids = new[] { "1", "2", "3", "4" };
            var sysUserMods = await _virtualDbContext.Set<SysUserMod>().Where(o => new List<string> { "1", "2", "3", "4" }.Contains(o.Id)).ToListAsync();
            foreach (var id in ids)
            {
                Assert.Contains(sysUserMods, o => o.Id == id);
            }

            Assert.DoesNotContain(sysUserMods, o => o.Age > 4);
        }

        [Fact]
        public async Task ToList_Id_Eq_Test()
        {
            var id = 3;
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == id.ToString()).ToListAsync();
            Assert.Single(mods);
            var mods1 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "4").ToListAsync();
            Assert.Single(mods1);
            Assert.Equal("3", mods[0].Id);
        }

        [Fact]
        public async Task ToList_Id_Not_Eq_Test()
        {
            var methodValue = new MethodValue() { AA = "7" };
            var mods123 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == methodValue.GetAa()).FirstOrDefaultAsync();
            Assert.NotNull(mods123);
            Assert.Equal(mods123.Id, "7");
            var mods12 = await _virtualDbContext.Set<SysUserMod>().Where(o => new List<string> { "3", "4" }.Contains(o.Id)).ToListAsync();
            Assert.Contains(mods12, o => o.Id == "3" || o.Id == "4");
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").ToListAsync();
            Assert.Equal(999, mods.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
        }
        public class MethodValue
        {
            public string AA { get; set; }

            public string GetAa()
            {
                return AA;
            }
        }

        [Fact]
        public async Task ToList_Id_Not_Eq_Skip_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").OrderBy(o => o.Age).Skip(2).ToListAsync();
            Assert.Equal(997, mods.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
            Assert.Equal(4, mods[0].Age);
            Assert.Equal(5, mods[1].Age);

            var modsDesc = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").OrderByDescending(o => o.Age).Skip(13).ToListAsync();
            Assert.Equal(986, modsDesc.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
            Assert.Equal(987, modsDesc[0].Age);
            Assert.Equal(986, modsDesc[1].Age);
        }

        [Fact]
        public async Task ToList_Name_Eq_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_3").ToListAsync();
            Assert.Single(mods);
            Assert.Equal("3", mods[0].Id);
        }

        [Fact]
        public async Task ToList_Id_Eq_Not_In_Db_Test()
        {
            var mod1s = await _virtualDbContext.Set<SysUserMod>().Where(o =>string.Compare(((IId)o).Id,"1001") ==0).ToListAsync();
            var mod2s = await _virtualDbContext.Set<SysUserMod>().Where(o => string.Compare("1001", o.Id) == 0).ToListAsync();
            var mod3s = await _virtualDbContext.Set<SysUserMod>().Where(o => "1001".CompareTo(o.Id)==0).ToListAsync();
            var mod4s = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id.CompareTo("1001") == 0).ToListAsync();
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "1001").ToListAsync();
            Assert.Empty(mods);
        }

        [Fact]
        public async Task ToList_Name_Eq_Not_In_Db_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1001").ToListAsync();
            Assert.Empty(mods);
        }

        [Fact]
        public async Task FirstOrDefault_Order_By_Id_Test()
        {
            var sysUserModAge = await _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).FirstOrDefaultAsync();
            Assert.True(sysUserModAge != null && sysUserModAge.Id == "1");
            var sysUserModAgeDesc = await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).FirstOrDefaultAsync();
            Assert.True(sysUserModAgeDesc != null && sysUserModAgeDesc.Id == "1000");
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Id).FirstOrDefaultAsync();
            Assert.True(sysUserMod != null && sysUserMod.Id == "1");

            var sysUserModDesc = await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Id).FirstOrDefaultAsync();
            Assert.True(sysUserModDesc != null && sysUserModDesc.Id == "999");

        }


        [Fact]
        public async Task FirstOrDefault2()
        {
            var sysUserModabxxxxx1 = await _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth >= 202102).FirstOrDefaultAsync();
            var sysUserModabxxxxx71 = await _virtualDbContext.Set<SysUserSalary>().Where(ox => ox.DateOfMonth >= 202102).FirstOrDefaultAsync();
            var xxxx1 = 202102;
            var sysUserModabxxxxx72 = await _virtualDbContext.Set<SysUserSalary>().Where(ox => ox.DateOfMonth >= xxxx1).FirstOrDefaultAsync();
            var sysUserModabxxxxx2 = await _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth >= 202101).FirstOrDefaultAsync();
            var sysUserModabxxxxx3 = await _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth > 202102).FirstOrDefaultAsync();
            var sysUserModabxxxxx4 = await _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth == 202102).FirstOrDefaultAsync();
            var sysUserModabxxxxx5 = await _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth < 202102).FirstOrDefaultAsync();
            var sysUserModabxxxxx6 = await _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth <= 202102).FirstOrDefaultAsync();
            var next = "1";
            var sysUserMod1 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == next).FirstOrDefaultAsync();
            var sysUserModabxxx = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_2").FirstOrDefaultAsync();
            var sysUserModabxxx11 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_2" || o.Name == "name_3").FirstOrDefaultAsync();
            var x = new Object[] { "1", "2" };
            var sysUserModab = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id.Equals("1")).FirstOrDefaultAsync();
            Assert.NotNull(sysUserModab);
            Assert.True(sysUserModab.Id == "1");
            var sysUserModaa = await _virtualDbContext.Set<SysUserMod>().Where(o => "1".Equals(o.Id)).FirstOrDefaultAsync();
            Assert.NotNull(sysUserModaa);
            Assert.True(sysUserModaa.Id == "1");
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "1").FirstOrDefaultAsync();
            Assert.NotNull(sysUserMod);
            Assert.True(sysUserMod.Id == "1");
            Assert.Equal(sysUserModaa, sysUserMod);
            var sysUserModxx = await _virtualDbContext.Set<SysUserMod>().Where(o => x.Contains(o.Id)).FirstOrDefaultAsync();
            Assert.NotNull(sysUserModxx);
            Assert.True(x.Contains(sysUserModxx.Id));
            Assert.NotNull(sysUserMod);
            var userMod = _virtualDbContext.Set<SysUserMod>().Find("1");
            Assert.Equal(sysUserMod, userMod);
            Assert.True(sysUserMod.Id == "1");
            var user198 = await _virtualDbContext.Set<SysUserMod>().FirstOrDefaultAsync(o => o.Id == "198");
            Assert.True(user198.Id == "198");
            var userId198 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Select(o => o.Id).FirstOrDefaultAsync();
            Assert.Equal(userId198, "198");
        }

        [Fact]
        public async Task FirstOrDefault3()
        {
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_2").FirstOrDefaultAsync();
            Assert.NotNull(sysUserMod);
            Assert.Equal("2", sysUserMod.Id);
            var sysUserMod2 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_2").Select(o => new
            {
                Name=o.Name
            }).FirstOrDefaultAsync();
            var sysUserMod1 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_2").Select(o => new TestClass(o.Name,""){Id = o.Id}).FirstOrDefaultAsync();
            Assert.NotNull(sysUserMod1);

        }
        public class TestClass
        {
            public string Name { get; }
            public string Aa { get; }
            public string Id { get; set; }

            public TestClass(string name,string aa)
            {
                Name = name;
                Aa = aa;
            }
        }

        [Fact]
        public async Task FirstOrDefault4()
        {
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "1").FirstOrDefaultAsync();
            Assert.NotNull(sysUserMod);
            Assert.True(sysUserMod.Id != "1");
        }

        [Fact]
        public async Task FirstOrDefault5()
        {
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1001").FirstOrDefaultAsync();
            Assert.Null(sysUserMod);
        }


        [Fact]
        public async Task Count_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1000").CountAsync();
            Assert.Equal(1, a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").CountAsync();
            Assert.Equal(999, b);
        }

        [Fact]
        public async Task Sum_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().SumAsync(o => o.Age);
            var expected = 0;
            for (int i = 1; i <= 1000; i++)
            {
                expected += i;
            }

            Assert.Equal(expected, a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").SumAsync(o => o.Age);
            Assert.Equal(expected - 1000, b);
        }

        [Fact]
        public async Task Max_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().MaxAsync(o => o.Age);
            Assert.Equal(1000, a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").MaxAsync(o => o.Age);
            Assert.Equal(999, b);
            var c = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age < 500).MaxAsync(o => o.Age);
            Assert.Equal(499, c);
            var e = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age <= 500).MaxAsync(o => o.Age);
            Assert.Equal(500, e);
        }

        [Fact]
        public async Task Max_Join_Test()
        {
            var queryable = (from u in _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "300")
                             join salary in _virtualDbContext.Set<SysUserSalary>()
                                 on u.Id equals salary.UserId
                             select new
                             {
                                 Salary = salary.Salary,
                                 DateOfMonth = salary.DateOfMonth,
                                 Name = u.Name
                             });
            var maxSalary = await queryable.MaxAsync(o => o.Salary);
            Assert.Equal(1390000, maxSalary);
        }

        [Fact]
        public async Task Min_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().MinAsync(o => o.Age);
            Assert.Equal(1, a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1").MinAsync(o => o.Age);
            Assert.Equal(2, b);
            var c = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age > 500).MinAsync(o => o.Age);
            Assert.Equal(501, c);
            var e = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age >= 500).MinAsync(o => o.Age);
            Assert.Equal(500, e);
        }

        [Fact]
        public async Task Any_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().AnyAsync(o => o.Age == 100);
            Assert.True(a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1").AnyAsync(o => o.Age == 1);
            Assert.False(b);
            var c = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age > 500).AnyAsync(o => o.Age <= 500);
            Assert.False(c);
            var e = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age >= 500).AnyAsync(o => o.Age <= 500);
            Assert.True(e);
        }

        [Fact]
        public async Task Group_Test()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var group = await (from u in _virtualDbContext.Set<SysUserSalary>()
                    .Where(o => ids.Contains(o.UserId) && dateOfMonths.Contains(o.DateOfMonth))
                               group u by new
                               {
                                   UId = u.UserId
                               }
                into g
                               select new
                               {
                                   GroupUserId = g.Key.UId,
                                   Count = g.Count(),
                                   TotalSalary = g.Sum(o => o.Salary),
                                   AvgSalary = g.Average(o => o.Salary),
                                   AvgSalaryDecimal = g.Average(o => o.SalaryDecimal),
                                   MinSalary = g.Min(o => o.Salary),
                                   MaxSalary = g.Max(o => o.Salary)
                               }).ToListAsync();
            Assert.Equal(2, group.Count);
            Assert.Equal(2, group[0].Count);
            Assert.Equal(2260000, group[0].TotalSalary);
            Assert.Equal(1130000, group[0].AvgSalary);
            Assert.Equal(11300, group[0].AvgSalaryDecimal);
            Assert.Equal(1120000, group[0].MinSalary);
            Assert.Equal(1140000, group[0].MaxSalary);
        }

        [Fact]
        public async Task Group_Test1()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var dateTime = DateTime.Now;

            var sql = from u in _virtualDbContext.Set<SysUserSalary>()
                      group u by u.UserId
                into g
                      select new
                      {
                          UI = g.Key,
                          x=g.Sum(o=>o.SalaryDecimal),
                          Now = dateTime
                      };
            var listAsync = await sql.ToListAsync();
            var group = await (from u in _virtualDbContext.Set<SysUserSalary>()
                     .Where(o => ids.Contains(o.UserId) && dateOfMonths.Contains(o.DateOfMonth))
                               group u by new
                               {
                                   UId = u.UserId
                               }
                 into g
                               select new
                               {
                                   GroupUserId = g.Key.UId,
                                   Count = g.Count(),
                                   TotalSalary = g.Sum(o => o.Salary),
                                   AvgSalary = g.Average(o => o.Salary),
                                   AvgSalaryDecimal = g.Average(o => o.SalaryDecimal),
                                   MinSalary = g.Min(o => o.Salary),
                                   MaxSalary = g.Max(o => o.Salary)
                               }).ToListAsync();
            Assert.Equal(2, group.Count);
            Assert.Equal(2, group[0].Count);
            Assert.Equal(2260000, group[0].TotalSalary);
            Assert.Equal(1130000, group[0].AvgSalary);
            Assert.Equal(11300, group[0].AvgSalaryDecimal);
            Assert.Equal(1120000, group[0].MinSalary);
            Assert.Equal(1140000, group[0].MaxSalary);
        }
        //[Fact]
        //public async Task Group_Recently_Test()
        //{
        //    //var list =(from us in _virtualDbContext.Set<SysUserSalary>().Where(o => ids.Contains(o.UserId))
        //    //          group us by new
        //    //              {
        //    //                  UserId=us.UserId
        //    //              }
        //    //              into g
        //    //          select new
        //    //              {
        //    //                  UserId=g.Key.UserId,
        //    //              DateOfMonth = g.Max(o=>o.DateOfMonth)
        //    //              }).ToList();
        //    //var y = list;

        //    var ids = new List<string>(){ "200", "300" };
        //    List<SysUserSalary> result = new List<SysUserSalary>(ids.Count);
        //    var routeFilter = new List<SysUserSalary>().AsQueryable().Where(o => ids.Contains(o.UserId));
        //    //��ȡ��·��ʱ�䵹��
        //    var tableRouteResults = _tableRouteRuleEngineFactory.Route(routeFilter)
        //        .Select(o => o.ReplaceTables.First().Tail).OrderByDescending(o => o).ToList();
        //    foreach (var tableRouteResult in tableRouteResults)
        //    {
        //        if(ids.IsEmpty())
        //            break;
        //        using (_shardingRouteManager.CreateScope())
        //        {
        //            _shardingRouteManager.Current.TryCreateOrAddMustTail<SysUserSalary>(tableRouteResult);
        //            var queryable = _virtualDbContext.Set<SysUserSalary>().Where(o => ids.Contains(o.UserId))
        //                .GroupBy(o => new { o.UserId }, i => i,
        //                    (i, u) => new {
        //                        Data = u.OrderByDescending(o => o.DateOfMonth).FirstOrDefault()
        //                    });
        //            var r =await queryable.ToListAsync();
        //            result.AddRange(r.Select(o=>o.Data));
        //            var removeUserIds = result.Select(u => u.UserId).ToHashSet();
        //            ids.RemoveAll(o => removeUserIds.Contains(o));
        //        }
        //    }
        //}

        [Fact]
        public async Task OrderCountTest()
        {
            var asyncCount = await _virtualDbContext.Set<Order>().CountAsync();
            Assert.Equal(320, asyncCount);
            var syncCount = _virtualDbContext.Set<Order>().Count();
            Assert.Equal(320, syncCount);

            var countA = await _virtualDbContext.Set<Order>().CountAsync(o => o.Area == "A");
            var countB = await _virtualDbContext.Set<Order>().CountAsync(o => o.Area == "B");
            var countC = await _virtualDbContext.Set<Order>().CountAsync(o => o.Area == "C");
            Assert.Equal(320, countA + countB + countC);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount = await _virtualDbContext.Set<Order>().Where(o => o.CreateTime >= fourBegin && o.CreateTime < fiveBegin).CountAsync();
            Assert.Equal(30, fourCount);
        }
        [Fact]
        public async Task OrderFirstTest()
        {
            var threeMonth = new DateTime(2021, 3, 1);
            var order = await _virtualDbContext.Set<Order>().FirstOrDefaultAsync(o => o.CreateTime == threeMonth);//��59�� 1��31��2��28��
            Assert.NotNull(order);
            Assert.Equal(59, order.Money);
            Assert.Equal("C", order.Area);
        }
        [Fact]
        public async Task OrderOrderTest()
        {
            var orders = await _virtualDbContext.Set<Order>().OrderBy(o => o.CreateTime).ToListAsync();
            Assert.Equal(320, orders.Count);
            var i = 0;
            foreach (var order in orders)
            {
                Assert.Equal(i, order.Money);
                i++;
            }

            var threeMonth = new DateTime(2021, 3, 1);
            var orderPage = await _virtualDbContext.Set<Order>().Where(o => o.CreateTime > threeMonth).OrderByDescending(o => o.CreateTime).ToShardingPageAsync(1, 20);
            Assert.Equal(20, orderPage.Data.Count);
            Assert.Equal(260, orderPage.Total);


            var j = 319;
            foreach (var order in orderPage.Data)
            {
                Assert.Equal(j, order.Money);
                j--;
            }


            var orderPage1 = await _virtualDbContext.Set<Order>().Where(o => o.CreateTime > threeMonth).OrderBy(o => o.CreateTime).ToShardingPageAsync(1, 20);
            Assert.Equal(20, orderPage1.Data.Count);
            Assert.Equal(260, orderPage1.Total);


            var j1 = 60;
            foreach (var order in orderPage1.Data)
            {
                Assert.Equal(j1, order.Money);
                j1++;
            }
        }

        [Fact]
        public async Task LogDayCountTest()
        {
            var countAsync = await _virtualDbContext.Set<LogDay>().CountAsync();
            Assert.Equal(3000, countAsync);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount = await _virtualDbContext.Set<LogDay>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).CountAsync();
            Assert.Equal(300, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogDay>("20210102");
                var countAsync1 = await _virtualDbContext.Set<LogDay>().CountAsync();
                Assert.Equal(10, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogDay>("20210103", "20210104");
                var countAsync2 = await _virtualDbContext.Set<LogDay>().CountAsync();
                Assert.Equal(20, countAsync2);
            }
        }
        [Fact]
        public void LogDayTableSeparatorTest()
        {
            var tableRoute = _tableRouteManager.GetRoute(typeof(LogDay));
            var virtualTableName =tableRoute.EntityMetadata.LogicTableName;
            Assert.Equal(nameof(LogDay), virtualTableName);

            Assert.True(string.IsNullOrWhiteSpace(tableRoute.EntityMetadata.TableSeparator));
            var entityMetadata = _entityMetadataManager.TryGet<LogDay>();
            Assert.NotNull(entityMetadata);
            var isShardingTable = entityMetadata.IsShardingTable();
            Assert.True(isShardingTable);
            var isShardingDataSource = entityMetadata.IsShardingDataSource();
            Assert.False(isShardingDataSource);
            var emptySeparator = string.IsNullOrWhiteSpace(entityMetadata.TableSeparator);
            Assert.True(emptySeparator);
            Assert.Null(entityMetadata.AutoCreateDataSourceTable);
        }

        [Fact]
        public async Task LogDayShardingPage()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var page = await _virtualDbContext.Set<LogDay>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                 .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(300, page.Total);

            var page1 = await _virtualDbContext.Set<LogDay>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(300, page1.Total);
        }

        [Fact]
        public async Task Order_Average()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var moneyAverage = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin&& o.CreateTime <= fiveBegin).Select(o => o.Money).AverageAsync();
            Assert.Equal(105, moneyAverage);

            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("C");
                var sum = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).SumAsync();
                Assert.Equal(0, sum);
                var sum1 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (long?)o.Money).SumAsync();
                Assert.Equal(0, sum1);
                var sum2 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (int)o.Money).SumAsync();
                Assert.Equal(0, sum2);
                var sum3 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (int?)o.Money).SumAsync();
                Assert.Equal(0, sum3);
                var sum4 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (decimal)o.Money).SumAsync();
                Assert.Equal(0, sum4);
                var sum5 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (decimal?)o.Money).SumAsync();
                Assert.Equal(0, sum5);
                var sum6 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (double)o.Money).SumAsync();
                Assert.Equal(0, sum6);
                var sum7 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (double?)o.Money).SumAsync();
                Assert.Equal(0, sum7);
                var sum8 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (float)o.Money).SumAsync();
                Assert.Equal(0, sum8);
                var sum9 = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (float?)o.Money).SumAsync();
                Assert.Equal(0, sum9);
            }
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintDataSource<Order>("C");
                var sum = await _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).SumAsync();
                Assert.Equal(0, sum);
            }

            var max = await _virtualDbContext.Set<Order>().MaxAsync(o => o.Money);
            Assert.Equal(319, max);
            var all = await _virtualDbContext.Set<Order>().AllAsync(o => o.Money <= 321);
            Assert.True(all);
            var longCount = await _virtualDbContext.Set<Order>().LongCountAsync();
            Assert.Equal(320, longCount);
        }

        [Fact]
        public async Task Order_Max()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;


            var moneyMax = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => o.Money).MaxAsync();
            Assert.Equal(120, moneyMax);
            var moneyMax1 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (long?)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax1);
            var moneyMax2 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (int)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax2);
            var moneyMax3 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (int?)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax3);
            var moneyMax4 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (double)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax4);
            var moneyMax5 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (double?)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax5);
            var moneyMax6 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (float)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax6);
            var moneyMax7 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (float?)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax7);
            var moneyMax8 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (decimal)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax8);
            var moneyMax9 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (decimal?)o.Money).MaxAsync();
            Assert.Equal(120, moneyMax9);

            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("C");
                try
                {
                    var max = await _virtualDbContext.Set<Order>()
                        .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).MaxAsync();
                }
                catch (Exception e)
                {
                    Assert.True(typeof(InvalidOperationException) == e.GetType() || typeof(TargetInvocationException) == e.GetType());
                    Assert.True(e.Message.Contains("contains") || e.InnerException.Message.Contains("contains"));
                }
            }
        }
        [Fact]
        public async Task Order_Min()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;


            var moneyMin = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => o.Money).MinAsync();
            Assert.Equal(90, moneyMin);
            var moneyMin1 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (long?)o.Money).MinAsync();
            Assert.Equal(90, moneyMin1);
            var moneyMin2 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (int)o.Money).MinAsync();
            Assert.Equal(90, moneyMin2);
            var moneyMin3 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (int?)o.Money).MinAsync();
            Assert.Equal(90, moneyMin3);
            var moneyMin4 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (float)o.Money).MinAsync();
            Assert.Equal(90, moneyMin4);
            var moneyMin5 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (float?)o.Money).MinAsync();
            Assert.Equal(90, moneyMin5);
            var moneyMin6 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (double)o.Money).MinAsync();
            Assert.Equal(90, moneyMin6);
            var moneyMin7 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (double?)o.Money).MinAsync();
            Assert.Equal(90, moneyMin7);
            var moneyMin8 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (decimal)o.Money).MinAsync();
            Assert.Equal(90, moneyMin8);
            var moneyMin9 = await _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (decimal?)o.Money).MinAsync();
            Assert.Equal(90, moneyMin9);

            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("C");
                try
                {
                    var max = await _virtualDbContext.Set<Order>()
                        .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).MinAsync();
                }
                catch (Exception e)
                {
                    Assert.True(typeof(InvalidOperationException) == e.GetType() || typeof(TargetInvocationException) == e.GetType());
                    Assert.True(e.Message.Contains("contains") || e.InnerException.Message.Contains("contains"));
                }
            }
        }

        [Fact]
        public async Task Order_Entity()
        {
            var x = await _virtualDbContext.Set<Order>().OrderBy(o => o.Money).LastOrDefaultAsync();
            Assert.NotNull(x);
            Assert.Equal(319, x.Money);
            var x1 = await _virtualDbContext.Set<Order>().OrderBy(o => o.Money).LastAsync();
            Assert.Equal(x, x1);
            var y = await _virtualDbContext.Set<Order>().OrderBy(o => o.Money).FirstOrDefaultAsync();
            Assert.NotNull(y);
            Assert.Equal(0, y.Money);
            var y1 = await _virtualDbContext.Set<Order>().OrderBy(o => o.Money).FirstAsync();
            Assert.Equal(y, y1);
            var z = await _virtualDbContext.Set<Order>().SingleOrDefaultAsync(o => o.Money == 13);
            var z1 = await _virtualDbContext.Set<Order>().SingleAsync(o => o.Money == 13);
            Assert.Equal(z, z1);
        }

        [Fact]
        public async Task OrderReadWrite()
        {
            //�л���ֻ�����ݿ⣬ֻ�����ݿ���ֻ������A����Դ��ȡB����Դ
            _virtualDbContext.ReadWriteSeparationReadOnly();
            var list = await _virtualDbContext.Set<Order>().Where(o => o.Money == 1).ToListAsync();
            Assert.Equal(2, list.Count);

            _virtualDbContext.ReadWriteSeparationWriteOnly();
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                var areaB = await _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefaultAsync();
                Assert.Null(areaB);
            }
            _virtualDbContext.ReadWriteSeparationReadOnly();
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                var areaB = await _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefaultAsync();
                Assert.NotNull(areaB);
            }

            _virtualDbContext.ReadWriteSeparationWriteOnly();
            using (_shardingReadWriteManager.CreateScope())
            {
                _shardingReadWriteManager.GetCurrent().SetReadWriteSeparation(100, true);
                using (_shardingRouteManager.CreateScope())
                {
                    _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                    var areaB = await _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefaultAsync();
                    Assert.NotNull(areaB);
                }
            }
            using (_shardingReadWriteManager.CreateScope())
            {
                _shardingReadWriteManager.GetCurrent().SetReadWriteSeparation(100, true);
                _virtualDbContext.ReadWriteSeparationWriteOnly();
                using (_shardingRouteManager.CreateScope())
                {
                    _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                    var areaB = await _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefaultAsync();
                    Assert.Null(areaB);
                }
            }
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                var areaB = await _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefaultAsync();
                Assert.Null(areaB);
            }
        }


        [Fact]
        public async Task LogWeekDateTimeCountTest()
        {
            var countAsync = await _virtualDbContext.Set<LogWeekDateTime>().CountAsync();
            Assert.Equal(300, countAsync);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount = await _virtualDbContext.Set<LogWeekDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).CountAsync();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogWeekDateTime>("20210419_25");
                var countAsync1 = await _virtualDbContext.Set<LogWeekDateTime>().CountAsync();
                Assert.Equal(7, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogWeekDateTime>("20210419_25", "20210426_02");
                var countAsync2 = await _virtualDbContext.Set<LogWeekDateTime>().CountAsync();
                Assert.Equal(14, countAsync2);
            }
        }

        [Fact]
        public async Task LogWeekDateTimeShardingPage()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var page = await _virtualDbContext.Set<LogWeekDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                 .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 = await _virtualDbContext.Set<LogWeekDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }

        [Fact]
        public async Task LogWeekTimeLongCountTest()
        {
            var countAsync = await _virtualDbContext.Set<LogWeekTimeLong>().CountAsync();
            Assert.Equal(300, countAsync);
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var fourCount = await _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).CountAsync();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogWeekTimeLong>("20210419_25");
                var countAsync1 = await _virtualDbContext.Set<LogWeekTimeLong>().CountAsync();
                Assert.Equal(7, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogWeekTimeLong>("20210419_25", "20210426_02");
                var countAsync2 = await _virtualDbContext.Set<LogWeekTimeLong>().CountAsync();
                Assert.Equal(14, countAsync2);
            }
        }

        [Fact]
        public async Task LogWeekDateLongShardingPage()
        {
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var page = await _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                 .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 = await _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }

        [Fact]
        public async Task LogYearTimeLongCountTest()
        {
            var countAsync = await _virtualDbContext.Set<LogYearDateTime>().CountAsync();
            Assert.Equal(600, countAsync);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount = await _virtualDbContext.Set<LogYearDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).CountAsync();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogYearDateTime>("2020");
                var countAsync1 = await _virtualDbContext.Set<LogYearDateTime>().CountAsync();
                Assert.Equal(366, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogYearDateTime>("2021");
                var countAsync2 = await _virtualDbContext.Set<LogYearDateTime>().CountAsync();
                Assert.Equal(234, countAsync2);
            }
        }
        [Fact]
        public async Task LogYearDateLongShardingPage()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var page = await _virtualDbContext.Set<LogYearDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 = await _virtualDbContext.Set<LogYearDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }
        [Fact]
        public async Task LogMonthTimeLongCountTest()
        {
            var countAsync = await _virtualDbContext.Set<LogMonthLong>().CountAsync();
            Assert.Equal(300, countAsync);
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var fourCount = await _virtualDbContext.Set<LogMonthLong>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).CountAsync();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogMonthLong>("202104");
                var countAsync1 = await _virtualDbContext.Set<LogMonthLong>().CountAsync();
                Assert.Equal(30, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogMonthLong>("202104", "202105");
                var countAsync2 = await _virtualDbContext.Set<LogMonthLong>().CountAsync();
                Assert.Equal(61, countAsync2);
            }
        }
        [Fact]
        public async Task LogMonthDateLongShardingPage()
        {
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var page = await _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 = await _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }
        [Fact]
        public async Task LogYearLongCountTest()
        {
            var countAsync = await _virtualDbContext.Set<LogYearLong>().CountAsync();
            Assert.Equal(300, countAsync);
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var fourCount = await _virtualDbContext.Set<LogYearLong>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).CountAsync();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogYearLong>("2021");
                var countAsync1 = await _virtualDbContext.Set<LogYearLong>().CountAsync();
                Assert.Equal(300, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);

        }
        [Fact]
        public async Task LogYearLongShardingPage()
        {
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var page = await _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 = await _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }
        [Fact]
        public async Task CrudTest()
        {
            var logNoSharding = new LogNoSharding()
            {
                Id = Guid.NewGuid().ToString("n"),
                Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                CreationTime = DateTime.Now
            };
            var logNoShardings = new List<LogNoSharding>()
            {
                new LogNoSharding()
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    CreationTime = DateTime.Now
                },
                new LogNoSharding()
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    CreationTime = DateTime.Now
                }

            };
            var logNoSharding1 = new LogNoSharding()
            {
                Id = Guid.NewGuid().ToString("n"),
                Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                CreationTime = DateTime.Now
            };
            var logNoSharding1s = new List<LogNoSharding>()
            {
                new LogNoSharding()
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    CreationTime = DateTime.Now
                },
                new LogNoSharding()
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    CreationTime = DateTime.Now
                }

            };
            using (var tran = await _virtualDbContext.Database.BeginTransactionAsync())
            {

                try
                {
                    await _virtualDbContext.AddAsync(logNoSharding);

                    await _virtualDbContext.AddRangeAsync(logNoShardings);

                    await _virtualDbContext.Set<LogNoSharding>().AddAsync(logNoSharding1);

                    await _virtualDbContext.Set<LogNoSharding>().AddRangeAsync(logNoSharding1s);
                    await _virtualDbContext.SaveChangesAsync();
                    await tran.CommitAsync();
                }
                catch (Exception e)
                {
                    await tran.RollbackAsync();
                }
            }
            logNoSharding.Body = DateTime.Now.ToString("yyyyMMdd");
            _virtualDbContext.Update(logNoSharding);

            logNoShardings.ForEach(o => o.Body = DateTime.Now.ToString("yyyyMMdd"));
            _virtualDbContext.UpdateRange(logNoShardings);

            logNoSharding1.Body = DateTime.Now.ToString("yyyyMMdd");
            _virtualDbContext.Set<LogNoSharding>().Update(logNoSharding1);

            logNoSharding1s.ForEach(o => o.Body = DateTime.Now.ToString("yyyyMMdd"));
            _virtualDbContext.Set<LogNoSharding>().UpdateRange(logNoSharding1s);
            await _virtualDbContext.SaveChangesAsync();


            _virtualDbContext.Remove(logNoSharding);

            _virtualDbContext.RemoveRange(logNoShardings);

            _virtualDbContext.Set<LogNoSharding>().Remove(logNoSharding1);

            logNoSharding1s.ForEach(o => o.Body = DateTime.Now.ToString("yyyyMMdd"));
            _virtualDbContext.Set<LogNoSharding>().RemoveRange(logNoSharding1s);
            await _virtualDbContext.SaveChangesAsync();
        }
        [Fact]
        public async Task CrudTest1()
        {
            var logNoSharding = new LogNoSharding()
            {
                Id = Guid.NewGuid().ToString("n"),
                Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                CreationTime = DateTime.Now
            };
            var logNoShardings = new List<LogNoSharding>()
            {
                new LogNoSharding()
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    CreationTime = DateTime.Now
                },
                new LogNoSharding()
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Body = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    CreationTime = DateTime.Now
                }

            };
            using (var tran = await _virtualDbContext.Database.BeginTransactionAsync())
            {

                try
                {
                    await _virtualDbContext.AddAsync((object)logNoSharding);

                    await _virtualDbContext.AddRangeAsync(logNoShardings.Select(o => (object)o).ToArray());

                    await _virtualDbContext.SaveChangesAsync();
                    await tran.CommitAsync();
                }
                catch (Exception e)
                {
                    await tran.RollbackAsync();
                }
            }
            logNoSharding.Body = DateTime.Now.ToString("yyyyMMdd");
            _virtualDbContext.Update((object)logNoSharding);

            logNoShardings.ForEach(o => o.Body = DateTime.Now.ToString("yyyyMMdd"));
            _virtualDbContext.UpdateRange(logNoShardings.Select(o => (object)o).ToArray());

            await _virtualDbContext.SaveChangesAsync();


            _virtualDbContext.Remove((object)logNoSharding);

            _virtualDbContext.RemoveRange(logNoShardings.Select(o => (object)o).ToArray());

            await _virtualDbContext.SaveChangesAsync();
        }



        [Fact]
        public async Task Int_ToList_All_Route_Test()
        {
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<SysUserModInt>("00");

                var mod00s = await _virtualDbContext.Set<SysUserModInt>().ToListAsync();
                Assert.Equal(333, mod00s.Count);
            }
            var mods = await _virtualDbContext.Set<SysUserModInt>().ToListAsync();
            Assert.Equal(1000, mods.Count);

            var modOrders1 = await _virtualDbContext.Set<SysUserModInt>().OrderBy(o => o.Age).ToListAsync();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }


            var modOrders2 = await _virtualDbContext.Set<SysUserModInt>().OrderByDescending(o => o.Age).ToListAsync();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }
        }
        [Fact]
        public async Task Int_ToList_All_Test()
        {

            var mods = await _virtualDbContext.Set<SysUserModInt>().ToListAsync();
            Assert.Equal(1000, mods.Count);

            var modOrders1 = await _virtualDbContext.Set<SysUserModInt>().OrderBy(o => o.Age).ToListAsync();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }

            var modOrders2 = await _virtualDbContext.Set<SysUserModInt>().OrderByDescending(o => o.Age).ToListAsync();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }



            var pageResult = await _virtualDbContext.Set<SysUserModInt>().Skip(10).Take(10).OrderByDescending(o => o.Age).ToListAsync();
            Assert.Equal(10, pageResult.Count);
            int pageDescAge = 990;
            foreach (var sysUserMod in pageResult)
            {
                Assert.Equal(pageDescAge, sysUserMod.Age);
                pageDescAge--;
            }
        }


        [Fact]
        public async Task LogDayLongCountTest()
        {
            var countAsync = await _virtualDbContext.Set<LogDayLong>().CountAsync();
            Assert.Equal(3000, countAsync);
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var fourCount = await _virtualDbContext.Set<LogDayLong>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).CountAsync();
            Assert.Equal(300, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogDayLong>("20210102");
                var countAsync1 = await _virtualDbContext.Set<LogDayLong>().CountAsync();
                Assert.Equal(10, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogDayLong>("20210103", "20210104");
                var countAsync2 = await _virtualDbContext.Set<LogDayLong>().CountAsync();
                Assert.Equal(20, countAsync2);
            }
        }

        [Fact]
        public async Task LogDayLongShardingPage()
        {
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var page = await _virtualDbContext.Set<LogDayLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                 .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(300, page.Total);

            var page1 = await _virtualDbContext.Set<LogDayLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPageAsync(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(300, page1.Total);
        }

        [Fact]
        public async Task AsRouteTest()
        {
            var countAsync3 = await _virtualDbContext.Set<LogMonthLong>().AsRoute(o =>
            {
                o.TryCreateOrAddMustTail<LogMonthLong>("202104");
            }).CountAsync();
            Assert.Equal(30, countAsync3);
            var countAsync2 = await _virtualDbContext.Set<LogYearLong>().AsRoute(o =>
            {
                o.TryCreateOrAddMustTail<LogYearLong>("2021");
            }).CountAsync();
            Assert.Equal(300, countAsync2);

            var countAsync4 = await _virtualDbContext.Set<LogWeekTimeLong>().AsRoute(o =>
            {
                o.TryCreateOrAddMustTail<LogWeekTimeLong>("20210419_25");
            }).CountAsync();
            Assert.Equal(7, countAsync4);

            var countAsync5 = await _virtualDbContext.Set<LogWeekTimeLong>().AsRoute(o =>
            {
                o.TryCreateOrAddHintTail<LogWeekTimeLong>("20210419_25", "20210426_02");
            }).CountAsync();
            Assert.Equal(14, countAsync5);


            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var sum = await _virtualDbContext.Set<Order>().AsRoute(o =>
                {
                    o.TryCreateOrAddHintDataSource<Order>("C");
                })
                .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).SumAsync();
            Assert.Equal(0, sum);
        }
        [Fact]
        public async Task QueryInner_Test()
        {
            var sysUserMods = _virtualDbContext.Set<SysUserMod>().Select(o=>o);
            var sysUserModInts = await _virtualDbContext.Set<SysUserModInt>().Where(o=>sysUserMods.Select(i=>i.Age).Any(i=>i==o.Age)).ToListAsync();
            Assert.Equal(1000, sysUserModInts.Count);
        }
        [Fact]
        public async Task Group_API_Test()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var group = await _virtualDbContext.Set<SysUserSalary>()
                .Where(o => ids.Contains(o.UserId) && dateOfMonths.Contains(o.DateOfMonth))
                .GroupBy(g => new { UId = g.UserId })
                .Select(g => new
                {

                    GroupUserId = g.Key.UId,
                    Count = g.Count(),
                    TotalSalary = g.Sum(o => o.Salary),
                    AvgSalary = g.Average(o => o.Salary),
                    AvgSalaryDecimal = g.Average(o => o.SalaryDecimal),
                    MinSalary = g.Min(o => o.Salary),
                    MaxSalary = g.Max(o => o.Salary)
                }).ToListAsync();
            Assert.Equal(2, group.Count);
            Assert.Equal(2, group[0].Count);
            Assert.Equal(2260000, group[0].TotalSalary);
            Assert.Equal(1130000, group[0].AvgSalary);
            Assert.Equal(11300, group[0].AvgSalaryDecimal);
            Assert.Equal(1120000, group[0].MinSalary);
            Assert.Equal(1140000, group[0].MaxSalary);
        }
        [Fact]
        public async Task Group_API_Test1()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var group = await _virtualDbContext.Set<SysUserSalary>()
                .Where(o => ids.Contains(o.UserId) && dateOfMonths.Contains(o.DateOfMonth))
                .GroupBy(g => new { UId = g.UserId })
                .Select(g => new
                {

                    GroupUserId = g.Key.UId,
                    Count = g.Count(),
                    TotalSalary = g.Sum(o => o.Salary),
                    AvgSalary = g.Average(o => o.Salary),
                    AvgSalaryDecimal = g.Average(o => o.SalaryDecimal),
                    MinSalary = g.Min(o => o.Salary),
                    MaxSalary = g.Max(o => o.Salary)
                }).OrderBy(o => o.TotalSalary).ToListAsync();
            Assert.Equal(2, group.Count);
            Assert.Equal(2, group[0].Count);
            Assert.Equal("200", group[0].GroupUserId);
            Assert.Equal(2260000, group[0].TotalSalary);
            Assert.Equal(1130000, group[0].AvgSalary);
            Assert.Equal(11300, group[0].AvgSalaryDecimal);
            Assert.Equal(1120000, group[0].MinSalary);
            Assert.Equal(1140000, group[0].MaxSalary);
        }
        [Fact]
        public async Task Group_API_Test2()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var group = await _virtualDbContext.Set<SysUserSalary>()
                .Where(o => ids.Contains(o.UserId) && dateOfMonths.Contains(o.DateOfMonth))
                .GroupBy(g => new { UId = g.UserId })
                .Select(g => new
                {

                    GroupUserId = g.Key.UId,
                    Count = g.Count(),
                    TotalSalary = g.Sum(o => o.Salary),
                    AvgSalary = g.Average(o => o.Salary),
                    AvgSalaryDecimal = g.Average(o => o.SalaryDecimal),
                    MinSalary = g.Min(o => o.Salary),
                    MaxSalary = g.Max(o => o.Salary)
                }).OrderByDescending(o => o.TotalSalary).ToListAsync();
            Assert.Equal(2, group.Count);
            Assert.Equal(2, group[0].Count);
            Assert.Equal("300", group[0].GroupUserId);
            Assert.Equal(2690000, group[0].TotalSalary);
            Assert.Equal(1345000, group[0].AvgSalary);
            Assert.Equal(13450, group[0].AvgSalaryDecimal);
            Assert.Equal(1330000, group[0].MinSalary);
            Assert.Equal(1360000, group[0].MaxSalary);
        }
    }
}