using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.ShardingPageExtensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.TableCreator;
using ShardingCore.Test5x.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Extensions.ShardingQueryableExtensions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using Xunit;

namespace ShardingCore.Test5x
{


    public class ShardingTestSync
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

        public ShardingTestSync(ShardingDefaultDbContext virtualDbContext, IShardingRuntimeContext shardingRuntimeContext, IConfiguration configuration)
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
        //
        //     var queryable1 = _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth >= 202102);
        //     var routeParseExpression1 = ShardingUtil.GetRouteParseExpression(queryable1, virtualTableRoute.EntityMetadata,
        //         (i, op,propertyName) => virtualTableRoute.GetRouteFilter(i, op,propertyName), true);
        //     var queryable2 = _virtualDbContext.Set<SysUserSalary>().Where(ox => ox.DateOfMonth >= 202102);
        //     var routeParseExpression2 = ShardingUtil.GetRouteParseExpression(queryable2, virtualTableRoute.EntityMetadata,
        //         (i, op,propertyName) => virtualTableRoute.GetRouteFilter(i, op,propertyName), true);
        //     var xxxx1 = 202102;
        //     var queryable3 = _virtualDbContext.Set<SysUserSalary>().Where(ox => ox.DateOfMonth >= xxxx1);
        //     var routeParseExpression3 = ShardingUtil.GetRouteParseExpression(queryable3, virtualTableRoute.EntityMetadata,
        //         (i, op,propertyName) => virtualTableRoute.GetRouteFilter(i, op,propertyName), true);
        //     var queryable4 = _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth >= 202101);
        //     var routeParseExpression4 = ShardingUtil.GetRouteParseExpression(queryable4, virtualTableRoute.EntityMetadata,
        //         (i, op,propertyName) => virtualTableRoute.GetRouteFilter(i, op,propertyName), true);
        //     var queryable5 = _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth > 202101);
        //     var routeParseExpression5 = ShardingUtil.GetRouteParseExpression(queryable5, virtualTableRoute.EntityMetadata,
        //         (i, op,propertyName) => virtualTableRoute.GetRouteFilter(i, op,propertyName), true);
        //     var queryable6 = _virtualDbContext.Set<SysUserSalary>().Where(o => o.DateOfMonth == 202101);
        //     var routeParseExpression6 = ShardingUtil.GetRouteParseExpression(queryable6, virtualTableRoute.EntityMetadata,
        //         (i, op,propertyName) => virtualTableRoute.GetRouteFilter(i, op,propertyName), true);
        //     var queryable7 = _virtualDbContext.Set<SysUserSalary>().Where(o => 202101 <= o.DateOfMonth);
        //     var routeParseExpression7 = ShardingUtil.GetRouteParseExpression(queryable7, virtualTableRoute.EntityMetadata,
        //         (i, op,propertyName) => virtualTableRoute.GetRouteFilter(i, op,propertyName), true);
        //     var queryable8 = _virtualDbContext.Set<SysUserSalary>().Where(o => 202101 == o.DateOfMonth);
        //     var routeParseExpression8 = ShardingUtil.GetRouteParseExpression(queryable8, virtualTableRoute.EntityMetadata,
        //         (i, op,propertyName) => virtualTableRoute.GetRouteFilter(i, op,propertyName), true);
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
        public void GenericTest()
        {
            var a = new DefaultPhysicDataSource("aaa", "aaa", true);
            var b = new DefaultPhysicDataSource("aaa", "aaa1", false);
            Assert.Equal(a, b);
            var x = new EntityMetadata(typeof(LogDay));
            //, "aa", typeof(ShardingDefaultDbContext), new List<PropertyInfo>(),null
            var y = new EntityMetadata(typeof(LogDay));
            //, "aa1", typeof(ShardingDefaultDbContext), new List<PropertyInfo>(),null
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
            var contains =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "300").Select(o => o.Id).Contains("300");
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

            _virtualDbContext.AddRange(logDays);
            var bulkShardingExpression = _virtualDbContext.BulkShardingExpression<ShardingDefaultDbContext, Order>(o => new[] { "A", "B" }.Contains(o.Area));
            Assert.Equal(2, bulkShardingExpression.Count);
            Assert.True(bulkShardingExpression.ContainsKey("A"));
            Assert.True(bulkShardingExpression.ContainsKey("B"));

            var bulkShardingTableExpression = _virtualDbContext.BulkShardingTableExpression<ShardingDefaultDbContext, SysUserMod>(o => o.Id == Guid.NewGuid().ToString());
            Assert.Equal(1, bulkShardingTableExpression.Count());

            var isShardingDbContext = _virtualDbContext.IsShardingDbContext();
            Assert.True(isShardingDbContext);
            var isShardingTableDbContext = _virtualDbContext.IsShardingTableDbContext();
            Assert.True(isShardingTableDbContext);
            var shardingDbContext = _virtualDbContext.GetType().IsShardingDbContext();
            Assert.True(shardingDbContext);
            var shardingTableDbContext = _virtualDbContext.GetType().IsShardingTableDbContext();
            Assert.True(shardingTableDbContext);

            var x1x1 = new ParallelTableGroupNode(new HashSet<ParallelTableComparerType>()
                { new ParallelTableComparerType(typeof(SysUserMod)), new ParallelTableComparerType(typeof(SysUserSalary)) });
            var x2x2 = new ParallelTableGroupNode(new HashSet<ParallelTableComparerType>()
                {  new ParallelTableComparerType(typeof(SysUserSalary)),new ParallelTableComparerType(typeof(SysUserMod)), });
            Assert.Equal(x1x1, x2x2);
            Assert.Equal(x1x1.GetHashCode(), x2x2.GetHashCode());

        }

        public class SequenceClass
        {
            public string Id { get; set; }
            public string T { get; set; }
        }

        [Fact]
        public void TestMultiShardingProperty()
        {

            var multiOrder =  _virtualDbContext.Set<MultiShardingOrder>().Where(o => o.Id == 232398109278351360).FirstOrDefault();
            Assert.NotNull(multiOrder);
            var longs = new[] { 232398109278351360, 255197859283087360 };
            var multiOrders =  _virtualDbContext.Set<MultiShardingOrder>().Where(o => longs.Contains(o.Id)).ToList();
            Assert.Equal(2, multiOrders.Count);
            var dateTime = new DateTime(2021, 11, 1);
            var multiOrder404 =  _virtualDbContext.Set<MultiShardingOrder>().Where(o => o.Id == 250345338962063360 && o.CreateTime < dateTime).FirstOrDefault();
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
        //[Fact]
        //public void Route_TEST()
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
        public void ToList_All_Route_Test()
        {
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<SysUserMod>("00");

                var mod00s =  _virtualDbContext.Set<SysUserMod>().ToList();
                Assert.Equal(333, mod00s.Count);
            }
            var mods =  _virtualDbContext.Set<SysUserMod>().ToList();
            Assert.Equal(1000, mods.Count);

            var modOrders1 =  _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToList();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }


            var modOrders2 =  _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToList();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }
        }
        [Fact]
        public void ToList_All_Test()
        {

            var mods =  _virtualDbContext.Set<SysUserMod>().ToList();
            Assert.Equal(1000, mods.Count);

            var modOrders1 =  _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToList();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }

            var modOrders2 =  _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToList();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }



            var pageResult =  _virtualDbContext.Set<SysUserMod>().Skip(10).Take(10).OrderByDescending(o => o.Age).ToList();
            Assert.Equal(10, pageResult.Count);
            int pageDescAge = 990;
            foreach (var sysUserMod in pageResult)
            {
                Assert.Equal(pageDescAge, sysUserMod.Age);
                pageDescAge--;
            }
            var skip10First =  _virtualDbContext.Set<SysUserMod>().Skip(10).OrderByDescending(o => o.Age).FirstOrDefault();
            Assert.Equal(skip10First, pageResult[0]);
        }

        [Fact]
        public void ToList_Join_Test()
        {
            var list =  (from u in _virtualDbContext.Set<SysUserMod>()
                              join salary in _virtualDbContext.Set<SysUserSalary>()
                                  on u.Id equals salary.UserId
                              select new
                              {
                                  u.Id,
                                  u.Age,
                                  Salary = salary.Salary,
                                  DateOfMonth = salary.DateOfMonth,
                                  Name = u.Name
                              }).ToList();

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
            var list1 =  queryable.ToList();
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
            var list3 = queryable1.ToList();
            Assert.Equal(1, list3.Count());
            Assert.Contains(list3, o => o.Name == "name_300");
        }

        [Fact]
        public void ToList_OrderBy_Asc_Desc_Test()
        {
            var modascs =  _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToList();
            Assert.Equal(1000, modascs.Count);
            var i = 1;
            foreach (var age in modascs)
            {
                Assert.Equal(i, age.Age);
                i++;
            }

            var moddescs =  _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToList();
            Assert.Equal(1000, moddescs.Count);
            var j = 1000;
            foreach (var age in moddescs)
            {
                Assert.Equal(j, age.Age);
                j--;
            }
        }

        [Fact]
        public void ToList_Id_In_Test()
        {
            var ids = new[] { "1", "2", "3", "4" };
            var sysUserMods =  _virtualDbContext.Set<SysUserMod>().Where(o => new List<string> { "1", "2", "3", "4" }.Contains(o.Id)).ToList();
            foreach (var id in ids)
            {
                Assert.Contains(sysUserMods, o => o.Id == id);
            }

            Assert.DoesNotContain(sysUserMods, o => o.Age > 4);
        }

        [Fact]
        public void ToList_Id_Eq_Test()
        {
            var id = 3;
            var mods =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == id.ToString()).ToList();
            Assert.Single(mods);
            var mods1 =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "4").ToList();
            Assert.Single(mods1);
            Assert.Equal("3", mods[0].Id);
        }

        [Fact]
        public void ToList_Id_Not_Eq_Test()
        {
            var mods =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").ToList();
            Assert.Equal(999, mods.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
        }

        [Fact]
        public void ToList_Id_Not_Eq_Skip_Test()
        {
            var mods =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").OrderBy(o => o.Age).Skip(2).ToList();
            Assert.Equal(997, mods.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
            Assert.Equal(4, mods[0].Age);
            Assert.Equal(5, mods[1].Age);

            var modsDesc =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").OrderByDescending(o => o.Age).Skip(13).ToList();
            Assert.Equal(986, modsDesc.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
            Assert.Equal(987, modsDesc[0].Age);
            Assert.Equal(986, modsDesc[1].Age);
        }

        [Fact]
        public void ToList_Name_Eq_Test()
        {
            var mods =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_3").ToList();
            Assert.Single(mods);
            Assert.Equal("3", mods[0].Id);
        }

        [Fact]
        public void ToList_Id_Eq_Not_In_Db_Test()
        {
            var mods =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "1001").ToList();
            Assert.Empty(mods);
        }

        [Fact]
        public void ToList_Name_Eq_Not_In_Db_Test()
        {
            var mods =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1001").ToList();
            Assert.Empty(mods);
        }

        [Fact]
        public void FirstOrDefault_Order_By_Id_Test()
        {
            var sysUserModAge =  _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).FirstOrDefault();
            Assert.True(sysUserModAge != null && sysUserModAge.Id == "1");
            var sysUserModAgeDesc =  _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).FirstOrDefault();
            Assert.True(sysUserModAgeDesc != null && sysUserModAgeDesc.Id == "1000");
            var sysUserMod =  _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Id).FirstOrDefault();
            Assert.True(sysUserMod != null && sysUserMod.Id == "1");

            var sysUserModDesc =  _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Id).FirstOrDefault();
            Assert.True(sysUserModDesc != null && sysUserModDesc.Id == "999");

        }

        [Fact]
        public void FirstOrDefault2()
        {

            var x = new Object[] { "1", "2" };
            var sysUserModab =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id.Equals("1")).FirstOrDefault();
            Assert.NotNull(sysUserModab);
            Assert.True(sysUserModab.Id == "1");
            var sysUserModaa =  _virtualDbContext.Set<SysUserMod>().Where(o => "1".Equals(o.Id)).FirstOrDefault();
            Assert.NotNull(sysUserModaa);
            Assert.True(sysUserModaa.Id == "1");
            var sysUserMod =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "1").FirstOrDefault();
            Assert.NotNull(sysUserMod);
            Assert.True(sysUserMod.Id == "1");
            var sysUserModxx =  _virtualDbContext.Set<SysUserMod>().Where(o => x.Contains(o.Id)).FirstOrDefault();
            Assert.NotNull(sysUserModxx);
            Assert.Equal(sysUserModaa, sysUserMod);
            Assert.True(x.Contains(sysUserModxx.Id));
            Assert.NotNull(sysUserMod);
            var userMod = _virtualDbContext.Set<SysUserMod>().Find("1");
            Assert.Equal(sysUserMod, userMod);
            Assert.True(sysUserMod.Id == "1");
            var user198 =  _virtualDbContext.Set<SysUserMod>().FirstOrDefault(o => o.Id == "198");
            Assert.True(user198.Id == "198");
            var userId198 =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Select(o => o.Id).FirstOrDefault();
            Assert.Equal(userId198, "198");

        }

        [Fact]
        public void FirstOrDefault3()
        {
            var sysUserMod =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_2").FirstOrDefault();
            Assert.NotNull(sysUserMod);
            Assert.Equal("2", sysUserMod.Id);

        }

        [Fact]
        public void FirstOrDefault4()
        {
            var sysUserMod =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "1").FirstOrDefault();
            Assert.NotNull(sysUserMod);
            Assert.True(sysUserMod.Id != "1");
        }

        [Fact]
        public void FirstOrDefault5()
        {
            var sysUserMod =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1001").FirstOrDefault();
            Assert.Null(sysUserMod);
        }

        [Fact]
        public void Count_Test()
        {
            var a =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1000").Count();
            Assert.Equal(1, a);
            var b =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").Count();
            Assert.Equal(999, b);
        }

        [Fact]
        public void Sum_Test()
        {
            var a =  _virtualDbContext.Set<SysUserMod>().Sum(o => o.Age);
            var expected = 0;
            for (int i = 1; i <= 1000; i++)
            {
                expected += i;
            }

            Assert.Equal(expected, a);
            var b =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").Sum(o => o.Age);
            Assert.Equal(expected - 1000, b);
        }

        [Fact]
        public void Max_Test()
        {
            var a =  _virtualDbContext.Set<SysUserMod>().Max(o => o.Age);
            Assert.Equal(1000, a);
            var b =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").Max(o => o.Age);
            Assert.Equal(999, b);
            var c =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Age < 500).Max(o => o.Age);
            Assert.Equal(499, c);
            var e =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Age <= 500).Max(o => o.Age);
            Assert.Equal(500, e);
        }

        [Fact]
        public void Max_Join_Test()
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
            var maxSalary =  queryable.Max(o => o.Salary);
            Assert.Equal(1390000, maxSalary);
        }

        [Fact]
        public void Min_Test()
        {
            var a =  _virtualDbContext.Set<SysUserMod>().Min(o => o.Age);
            Assert.Equal(1, a);
            var b =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1").Min(o => o.Age);
            Assert.Equal(2, b);
            var c =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Age > 500).Min(o => o.Age);
            Assert.Equal(501, c);
            var e =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Age >= 500).Min(o => o.Age);
            Assert.Equal(500, e);
        }

        [Fact]
        public void Any_Test()
        {
            var a =  _virtualDbContext.Set<SysUserMod>().Any(o => o.Age == 100);
            Assert.True(a);
            var b =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1").Any(o => o.Age == 1);
            Assert.False(b);
            var c =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Age > 500).Any(o => o.Age <= 500);
            Assert.False(c);
            var e =  _virtualDbContext.Set<SysUserMod>().Where(o => o.Age >= 500).Any(o => o.Age <= 500);
            Assert.True(e);
        }

        [Fact]
        public void Group_Test()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var group =  (from u in _virtualDbContext.Set<SysUserSalary>()
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
                               }).ToList();
            Assert.Equal(2, group.Count);
            Assert.Equal(2, group[0].Count);
            Assert.Equal(2260000, group[0].TotalSalary);
            Assert.Equal(1130000, group[0].AvgSalary);
            Assert.Equal(11300, group[0].AvgSalaryDecimal);
            Assert.Equal(1120000, group[0].MinSalary);
            Assert.Equal(1140000, group[0].MaxSalary);
        }

        [Fact]
        public void OrderCountTest()
        {
            var asyncCount =  _virtualDbContext.Set<Order>().Count();
            Assert.Equal(320, asyncCount);
            var syncCount = _virtualDbContext.Set<Order>().Count();
            Assert.Equal(320, syncCount);

            var countA =  _virtualDbContext.Set<Order>().Count(o => o.Area == "A");
            var countB =  _virtualDbContext.Set<Order>().Count(o => o.Area == "B");
            var countC =  _virtualDbContext.Set<Order>().Count(o => o.Area == "C");
            Assert.Equal(320, countA + countB + countC);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount =  _virtualDbContext.Set<Order>().Where(o => o.CreateTime >= fourBegin && o.CreateTime < fiveBegin).Count();
            Assert.Equal(30, fourCount);
        }
        [Fact]
        public void OrderFirstTest()
        {
            var threeMonth = new DateTime(2021, 3, 1);
            var order =  _virtualDbContext.Set<Order>().FirstOrDefault(o => o.CreateTime == threeMonth);//第59条 1月31天2月28天
            Assert.NotNull(order);
            Assert.Equal(59, order.Money);
            Assert.Equal("C", order.Area);
        }
        [Fact]
        public void OrderOrderTest()
        {
            var orders =  _virtualDbContext.Set<Order>().OrderBy(o => o.CreateTime).ToList();
            Assert.Equal(320, orders.Count);
            var i = 0;
            foreach (var order in orders)
            {
                Assert.Equal(i, order.Money);
                i++;
            }

            var threeMonth = new DateTime(2021, 3, 1);
            var orderPage =  _virtualDbContext.Set<Order>().Where(o => o.CreateTime > threeMonth).OrderByDescending(o => o.CreateTime).ToShardingPage(1, 20);
            Assert.Equal(20, orderPage.Data.Count);
            Assert.Equal(260, orderPage.Total);

            var j = 319;
            foreach (var order in orderPage.Data)
            {
                Assert.Equal(j, order.Money);
                j--;
            }
        }

        [Fact]
        public void LogDayCountTest()
        {
            var countAsync =  _virtualDbContext.Set<LogDay>().Count();
            Assert.Equal(3000, countAsync);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount =  _virtualDbContext.Set<LogDay>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).Count();
            Assert.Equal(300, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogDay>("20210102");
                var countAsync1 =  _virtualDbContext.Set<LogDay>().Count();
                Assert.Equal(10, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogDay>("20210103", "20210104");
                var countAsync2 =  _virtualDbContext.Set<LogDay>().Count();
                Assert.Equal(20, countAsync2);
            }
        }
        [Fact]
        public void LogDayTableSeparatorTest()
        {
            var virtualTableRoute = _tableRouteManager.GetRoute(typeof(LogDay));
            var virtualTableName = virtualTableRoute.EntityMetadata.LogicTableName;
            Assert.Equal(nameof(LogDay), virtualTableName);
            Assert.True(string.IsNullOrWhiteSpace(virtualTableRoute.EntityMetadata.TableSeparator));
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
        public void LogDayShardingPage()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var page =  _virtualDbContext.Set<LogDay>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                 .ToShardingPage(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(300, page.Total);

            var page1 = _virtualDbContext.Set<LogDay>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(300, page1.Total);

        }

        [Fact]
        public void Order_Average()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var moneyAverage =  _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => o.Money).Average();
            Assert.Equal(105, moneyAverage);

            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("C");
                var sum =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).Sum();
                Assert.Equal(0, sum);
                var sum1 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (long?)o.Money).Sum();
                Assert.Equal(0, sum1);
                var sum2 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (int)o.Money).Sum();
                Assert.Equal(0, sum2);
                var sum3 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (int?)o.Money).Sum();
                Assert.Equal(0, sum3);
                var sum4 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (decimal)o.Money).Sum();
                Assert.Equal(0, sum4);
                var sum5 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (decimal?)o.Money).Sum();
                Assert.Equal(0, sum5);
                var sum6 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (double)o.Money).Sum();
                Assert.Equal(0, sum6);
                var sum7 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (double?)o.Money).Sum();
                Assert.Equal(0, sum7);
                var sum8 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (float)o.Money).Sum();
                Assert.Equal(0, sum8);
                var sum9 =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => (float?)o.Money).Sum();
                Assert.Equal(0, sum9);
            }
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintDataSource<Order>("C");
                var sum =  _virtualDbContext.Set<Order>()
                    .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).Sum();
                Assert.Equal(0, sum);
            }

            var all =  _virtualDbContext.Set<Order>().All(o => o.Money <= 321);
            Assert.True(all);
            var longCount =  _virtualDbContext.Set<Order>().LongCount();
            Assert.Equal(320, longCount);
        }

        [Fact]
        public void Order_Max()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var moneyMax =  _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => o.Money).Max();
            Assert.Equal(120, moneyMax);
            var moneyMax1 =  _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (long?)o.Money).Max();
            Assert.Equal(120, moneyMax1);
            var moneyMax2 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (int)o.Money).Max();
            Assert.Equal(120, moneyMax2);
            var moneyMax3 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (int?)o.Money).Max();
            Assert.Equal(120, moneyMax3);
            var moneyMax4 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (double)o.Money).Max();
            Assert.Equal(120, moneyMax4);
            var moneyMax5 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (double?)o.Money).Max();
            Assert.Equal(120, moneyMax5);
            var moneyMax6 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (float)o.Money).Max();
            Assert.Equal(120, moneyMax6);
            var moneyMax7 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (float?)o.Money).Max();
            Assert.Equal(120, moneyMax7);
            var moneyMax8 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (decimal)o.Money).Max();
            Assert.Equal(120, moneyMax8);
            var moneyMax9 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (decimal?)o.Money).Max();
            Assert.Equal(120, moneyMax9);

            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("C");
                try
                {
                    var max =  _virtualDbContext.Set<Order>()
                        .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).Max();
                }
                catch (Exception e)
                {
                    Assert.True(typeof(InvalidOperationException) == e.GetType() || typeof(TargetInvocationException) == e.GetType());
                    Assert.True(e.Message.Contains("contains")|| e.InnerException.Message.Contains("contains"));
                }
            }
        }
        [Fact]
        public void Order_Min()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var moneyMin =  _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => o.Money).Min();
            Assert.Equal(90, moneyMin);
            var moneyMin1 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (long?)o.Money).Min();
            Assert.Equal(90, moneyMin1);
            var moneyMin2 =  _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (int)o.Money).Min();
            Assert.Equal(90, moneyMin2);
            var moneyMin3 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (int?)o.Money).Min();
            Assert.Equal(90, moneyMin3);
            var moneyMin4 =  _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (float)o.Money).Min();
            Assert.Equal(90, moneyMin4);
            var moneyMin5 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (float?)o.Money).Min();
            Assert.Equal(90, moneyMin5);
            var moneyMin6 =  _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (double)o.Money).Min();
            Assert.Equal(90, moneyMin6);
            var moneyMin7 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (double?)o.Money).Min();
            Assert.Equal(90, moneyMin7);
            var moneyMin8 =  _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (decimal)o.Money).Min();
            Assert.Equal(90, moneyMin8);
            var moneyMin9 = _virtualDbContext.Set<Order>()
                .Where(o => o.CreateTime >= fourBegin && o.CreateTime <= fiveBegin).Select(o => (decimal?)o.Money).Min();
            Assert.Equal(90, moneyMin9);

            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("C");
                try
                {
                    var max =  _virtualDbContext.Set<Order>()
                        .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).Min();
                }
                catch (Exception e)
                {
                    Assert.True(typeof(InvalidOperationException) == e.GetType() || typeof(TargetInvocationException) == e.GetType());
                    Assert.True(e.Message.Contains("contains") || e.InnerException.Message.Contains("contains"));
                }
            }
        }

        [Fact]
        public void Order_Entity()
        {
            var x =  _virtualDbContext.Set<Order>().OrderBy(o => o.Money).LastOrDefault();
            Assert.NotNull(x);
            Assert.Equal(319, x.Money);
            var x1 =  _virtualDbContext.Set<Order>().OrderBy(o => o.Money).Last();
            Assert.Equal(x, x1);
            var y =  _virtualDbContext.Set<Order>().OrderBy(o => o.Money).FirstOrDefault();
            Assert.NotNull(y);
            Assert.Equal(0, y.Money);
            var y1 =  _virtualDbContext.Set<Order>().OrderBy(o => o.Money).First();
            Assert.Equal(y, y1);
            var z =  _virtualDbContext.Set<Order>().SingleOrDefault(o => o.Money == 13);
            var z1 =  _virtualDbContext.Set<Order>().Single(o => o.Money == 13);
            Assert.Equal(z, z1);
        }
        [Fact]
        public void OrderReadWrite()
        {
            //切换到只读数据库，只读数据库又只配置了A数据源读取B数据源
            _virtualDbContext.ReadWriteSeparationReadOnly();
            var list =  _virtualDbContext.Set<Order>().Where(o => o.Money == 1).ToList();
            Assert.Equal(2, list.Count);

            _virtualDbContext.ReadWriteSeparationWriteOnly();
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                var areaB =  _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefault();
                Assert.Null(areaB);
            }
            _virtualDbContext.ReadWriteSeparationReadOnly();
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                var areaB =  _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefault();
                Assert.NotNull(areaB);
            }
            _virtualDbContext.ReadWriteSeparationWriteOnly();
            using (_shardingReadWriteManager.CreateScope())
            {
                _shardingReadWriteManager.GetCurrent().SetReadWriteSeparation(100, true);
                using (_shardingRouteManager.CreateScope())
                {
                    _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                    var areaB =  _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefault();
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
                    var areaB = _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefault();
                    Assert.Null(areaB);
                }
            }
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustDataSource<Order>("A");
                var areaB =  _virtualDbContext.Set<Order>().Where(o => o.Area == "B").FirstOrDefault();
                Assert.Null(areaB);
            }
        }
        [Fact]
        public void LogWeekDateTimeCountTest()
        {
            var countAsync =  _virtualDbContext.Set<LogWeekDateTime>().Count();
            Assert.Equal(300, countAsync);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount =  _virtualDbContext.Set<LogWeekDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).Count();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogWeekDateTime>("20210419_25");
                var countAsync1 =  _virtualDbContext.Set<LogWeekDateTime>().Count();
                Assert.Equal(7, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogWeekDateTime>("20210419_25", "20210426_02");
                var countAsync2 =  _virtualDbContext.Set<LogWeekDateTime>().Count();
                Assert.Equal(14, countAsync2);
            }
        }

        [Fact]
        public void LogWeekDateTimeShardingPage()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var page =  _virtualDbContext.Set<LogWeekDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                 .ToShardingPage(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 =  _virtualDbContext.Set<LogWeekDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }

        [Fact]
        public void LogWeekTimeLongCountTest()
        {
            var countAsync =  _virtualDbContext.Set<LogWeekTimeLong>().Count();
            Assert.Equal(300, countAsync);
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var fourCount =  _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).Count();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogWeekTimeLong>("20210419_25");
                var countAsync1 =  _virtualDbContext.Set<LogWeekTimeLong>().Count();
                Assert.Equal(7, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogWeekTimeLong>("20210419_25", "20210426_02");
                var countAsync2 =  _virtualDbContext.Set<LogWeekTimeLong>().Count();
                Assert.Equal(14, countAsync2);
            }
        }

        [Fact]
        public void LogWeekDateLongShardingPage()
        {
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var page =  _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                 .ToShardingPage(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 =  _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }
        [Fact]
        public void LogYearTimeLongCountTest()
        {
            var countAsync =  _virtualDbContext.Set<LogYearDateTime>().Count();
            Assert.Equal(600, countAsync);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount =  _virtualDbContext.Set<LogYearDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).Count();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogYearDateTime>("2020");
                var countAsync1 =  _virtualDbContext.Set<LogYearDateTime>().Count();
                Assert.Equal(366, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogYearDateTime>("2021");
                var countAsync2 =  _virtualDbContext.Set<LogYearDateTime>().Count();
                Assert.Equal(234, countAsync2);
            }
        }
        [Fact]
        public void LogYearDateLongShardingPage()
        {
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var page =  _virtualDbContext.Set<LogYearDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 =  _virtualDbContext.Set<LogYearDateTime>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }

        [Fact]
        public void LogMonthTimeLongCountTest()
        {
            var countAsync =  _virtualDbContext.Set<LogMonthLong>().Count();
            Assert.Equal(300, countAsync);
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var fourCount =  _virtualDbContext.Set<LogMonthLong>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).Count();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogMonthLong>("202104");
                var countAsync1 =  _virtualDbContext.Set<LogMonthLong>().Count();
                Assert.Equal(30, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogMonthLong>("202104", "202105");
                var countAsync2 =  _virtualDbContext.Set<LogMonthLong>().Count();
                Assert.Equal(61, countAsync2);
            }
        }
        [Fact]
        public void LogMonthDateLongShardingPage()
        {
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var page =  _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 =  _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }
        [Fact]
        public async Task LogYearLongCountTest()
        {
            var countAsync =  _virtualDbContext.Set<LogYearLong>().Count();
            Assert.Equal(300, countAsync);
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var fourCount =  _virtualDbContext.Set<LogYearLong>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).Count();
            Assert.Equal(30, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogYearLong>("2021");
                var countAsync1 =  _virtualDbContext.Set<LogYearLong>().Count();
                Assert.Equal(300, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
        }
        [Fact]
        public async Task LogYearLongShardingPage()
        {
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var page =  _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(31, page.Total);

            var page1 =  _virtualDbContext.Set<LogWeekTimeLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(31, page1.Total);
        }

        [Fact]
        public void CrudTest()
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
            using (var tran =  _virtualDbContext.Database.BeginTransaction())
            {

                try
                {
                     _virtualDbContext.Add(logNoSharding);

                     _virtualDbContext.AddRange(logNoShardings);

                     _virtualDbContext.Set<LogNoSharding>().Add(logNoSharding1);

                     _virtualDbContext.Set<LogNoSharding>().AddRange(logNoSharding1s);
                     _virtualDbContext.SaveChanges();
                     tran.Commit();
                }
                catch (Exception e)
                {
                     tran.Rollback();
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
             _virtualDbContext.SaveChanges();


            _virtualDbContext.Remove(logNoSharding);

            _virtualDbContext.RemoveRange(logNoShardings);

            _virtualDbContext.Set<LogNoSharding>().Remove(logNoSharding1);

            logNoSharding1s.ForEach(o => o.Body = DateTime.Now.ToString("yyyyMMdd"));
            _virtualDbContext.Set<LogNoSharding>().RemoveRange(logNoSharding1s);
             _virtualDbContext.SaveChanges();
        }
        [Fact]
        public void CrudTest1()
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
            using (var tran =  _virtualDbContext.Database.BeginTransaction())
            {

                try
                {
                     _virtualDbContext.Add((object)logNoSharding);

                     _virtualDbContext.AddRange(logNoShardings.Select(o => (object)o).ToArray());

                     _virtualDbContext.SaveChanges();
                     tran.Commit();
                }
                catch (Exception e)
                {
                     tran.Rollback();
                }
            }
        }


        [Fact]
        public void Int_ToList_All_Route_Test()
        {
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<SysUserModInt>("00");

                var mod00s = _virtualDbContext.Set<SysUserModInt>().ToList();
                Assert.Equal(333, mod00s.Count);
            }
            var mods = _virtualDbContext.Set<SysUserModInt>().ToList();
            Assert.Equal(1000, mods.Count);

            var modOrders1 = _virtualDbContext.Set<SysUserModInt>().OrderBy(o => o.Age).ToList();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }


            var modOrders2 = _virtualDbContext.Set<SysUserModInt>().OrderByDescending(o => o.Age).ToList();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }
        }
        [Fact]
        public void Int_ToList_All_Test()
        {

            var mods = _virtualDbContext.Set<SysUserModInt>().ToList();
            Assert.Equal(1000, mods.Count);

            var modOrders1 = _virtualDbContext.Set<SysUserModInt>().OrderBy(o => o.Age).ToList();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }

            var modOrders2 = _virtualDbContext.Set<SysUserModInt>().OrderByDescending(o => o.Age).ToList();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }



            var pageResult = _virtualDbContext.Set<SysUserModInt>().Skip(10).Take(10).OrderByDescending(o => o.Age).ToList();
            Assert.Equal(10, pageResult.Count);
            int pageDescAge = 990;
            foreach (var sysUserMod in pageResult)
            {
                Assert.Equal(pageDescAge, sysUserMod.Age);
                pageDescAge--;
            }
        }


        [Fact]
        public void LogDayLongCountTest()
        {
            var countAsync =  _virtualDbContext.Set<LogDayLong>().Count();
            Assert.Equal(3000, countAsync);
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var fourCount =  _virtualDbContext.Set<LogDayLong>().Where(o => o.LogTime >= fourBegin && o.LogTime < fiveBegin).Count();
            Assert.Equal(300, fourCount);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<LogDayLong>("20210102");
                var countAsync1 =  _virtualDbContext.Set<LogDayLong>().Count();
                Assert.Equal(10, countAsync1);
            }
            Assert.Null(_shardingRouteManager.Current);
            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddHintTail<LogDayLong>("20210103", "20210104");
                var countAsync2 =  _virtualDbContext.Set<LogDayLong>().Count();
                Assert.Equal(20, countAsync2);
            }
        }

        [Fact]
        public void LogDayLongShardingPage()
        {
            var fourBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 4, 1).Date);
            var fiveBegin = ShardingCoreHelper.ConvertDateTimeToLong(new DateTime(2021, 5, 1).Date);
            var page =  _virtualDbContext.Set<LogDayLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
                 .ToShardingPage(2, 10);
            Assert.Equal(10, page.Data.Count);
            Assert.Equal(300, page.Total);

            var page1 =  _virtualDbContext.Set<LogDayLong>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin)
                .ToShardingPage(2, 10);
            Assert.Equal(10, page1.Data.Count);
            Assert.Equal(300, page1.Total);
        }
        [Fact]
        public void AsRouteTest()
        {
            var countAsync3 =  _virtualDbContext.Set<LogMonthLong>().AsRoute(o =>
            {
                o.TryCreateOrAddMustTail<LogMonthLong>("202104");
            }).Count();
            Assert.Equal(30, countAsync3);
            var countAsync2 =  _virtualDbContext.Set<LogYearLong>().AsRoute(o =>
            {
                o.TryCreateOrAddMustTail<LogYearLong>("2021");
            }).Count();
            Assert.Equal(300, countAsync2);

            var countAsync4 =  _virtualDbContext.Set<LogWeekTimeLong>().AsRoute(o =>
            {
                o.TryCreateOrAddMustTail<LogWeekTimeLong>("20210419_25");
            }).Count();
            Assert.Equal(7, countAsync4);

            var countAsync5 =  _virtualDbContext.Set<LogWeekTimeLong>().AsRoute(o =>
            {
                o.TryCreateOrAddHintTail<LogWeekTimeLong>("20210419_25", "20210426_02");
            }).Count();
            Assert.Equal(14, countAsync5);


            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var sum =  _virtualDbContext.Set<Order>().AsRoute(o =>
                {
                    o.TryCreateOrAddHintDataSource<Order>("C");
                })
                .Where(o => o.CreateTime == fiveBegin).Select(o => o.Money).Sum();
            Assert.Equal(0, sum);
        }
        [Fact]
        public void QueryInner_Test()
        {
            var sysUserMods = _virtualDbContext.Set<SysUserMod>().Select(o => o);
            var sysUserModInts = _virtualDbContext.Set<SysUserModInt>().Where(o => sysUserMods.Select(i => i.Age).Any(i => i == o.Age)).ToList();
            Assert.Equal(1000, sysUserModInts.Count);
        }
        [Fact]
        public void Group_API_Test()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var group =  _virtualDbContext.Set<SysUserSalary>()
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
                }).ToList();
            Assert.Equal(2, group.Count);
            Assert.Equal(2, group[0].Count);
            Assert.Equal(2260000, group[0].TotalSalary);
            Assert.Equal(1130000, group[0].AvgSalary);
            Assert.Equal(11300, group[0].AvgSalaryDecimal);
            Assert.Equal(1120000, group[0].MinSalary);
            Assert.Equal(1140000, group[0].MaxSalary);
        }
        [Fact]
        public void Group_API_Test1()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var group =  _virtualDbContext.Set<SysUserSalary>()
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
                }).OrderBy(o => o.TotalSalary).ToList();
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
        public void Group_API_Test2()
        {
            var ids = new[] { "200", "300" };
            var dateOfMonths = new[] { 202111, 202110 };
            var group =  _virtualDbContext.Set<SysUserSalary>()
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
                }).OrderByDescending(o => o.TotalSalary).ToList();
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
