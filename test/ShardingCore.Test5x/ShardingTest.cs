using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.Extensions.ShardingPageExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.Test5x.Domain.Entities;
using Xunit;

namespace ShardingCore.Test5x
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 15 January 2021 17:22:10
    * @Email: 326308290@qq.com
    */
    public class ShardingTest
    {
        private readonly ShardingDefaultDbContext _virtualDbContext;
        private readonly IShardingRouteManager _shardingRouteManager;
        private readonly IConnectionStringManager<ShardingDefaultDbContext> _connectionStringManager;
        private readonly IConfiguration _configuration;
        private readonly IEntityMetadataManager<ShardingDefaultDbContext> _entityMetadataManager;
        private readonly IShardingComparer<ShardingDefaultDbContext> _shardingComparer;
        private readonly IVirtualDataSource<ShardingDefaultDbContext> _virtualDataSource;
        private readonly IVirtualTableManager<ShardingDefaultDbContext> _virtualTableManager;

        public ShardingTest(ShardingDefaultDbContext virtualDbContext,IShardingRouteManager shardingRouteManager, IConnectionStringManager<ShardingDefaultDbContext> connectionStringManager,IConfiguration configuration,
            IEntityMetadataManager<ShardingDefaultDbContext> entityMetadataManager,
            IShardingComparer<ShardingDefaultDbContext> shardingComparer,IVirtualDataSource<ShardingDefaultDbContext> virtualDataSource,
            IVirtualTableManager<ShardingDefaultDbContext> virtualTableManager)
        {
            _virtualDbContext = virtualDbContext;
            _shardingRouteManager = shardingRouteManager;
            _connectionStringManager = connectionStringManager;
            _configuration = configuration;
            this._entityMetadataManager = entityMetadataManager;
            _shardingComparer = shardingComparer;
            _virtualDataSource = virtualDataSource;
            _virtualTableManager = virtualTableManager;
        }

        [Fact]
        public void GenericTest()
        {
            var a = new DefaultPhysicDataSource("aaa","aaa",true);
            var b = new DefaultPhysicDataSource("aaa","aaa1",false);
            Assert.Equal(a,b);
            var x = new EntityMetadata(typeof(LogDay),"aa",typeof(ShardingDefaultDbContext),new List<PropertyInfo>());
            var y = new EntityMetadata(typeof(LogDay),"aa1",typeof(ShardingDefaultDbContext),new List<PropertyInfo>());
            Assert.Equal(x, y);
            var dateTime = new DateTime(2021,1,1);
            var logDays = Enumerable.Range(0,100).Select(o=>new LogDay(){Id = Guid.NewGuid(),LogLevel = "info",LogBody = o.ToString(),LogTime = dateTime.AddDays(o)}).ToList();
            var bulkShardingTableEnumerable = _virtualDbContext.BulkShardingTableEnumerable(logDays);
            Assert.Equal(100, bulkShardingTableEnumerable.Count);
            var bulkShardingEnumerable = _virtualDbContext.BulkShardingEnumerable(logDays);
            Assert.Equal(1, bulkShardingEnumerable.Count);
            foreach (var (key, value) in bulkShardingEnumerable)
            {
                Assert.Equal(100, value.Count);
            }
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
            Assert.True(compare0>0);
            //asc x<y db compare  uniqueidentifier
            var compare1 = _shardingComparer.Compare(x, y, true);
            Assert.True(compare1 < 0);
        }
        [Fact]
        public void TestConnectionStringManager()
        {
            var connectionString = _connectionStringManager.GetConnectionString(_virtualDataSource.DefaultDataSourceName);
            Assert.Equal(connectionString, "Data Source=localhost;Initial Catalog=ShardingCoreDBA;Integrated Security=True;");
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
        }

        [Fact]
        public async Task ToList_Join_Test()
        {
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

            var list2 = list.OrderBy(o=>o.Age).Select(o=>o.Age).Distinct().ToList();
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
            var ids = new[] {"1", "2", "3", "4"};
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
            var id= 3;
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == id.ToString()).ToListAsync();
            Assert.Single(mods);
            var mods1 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "4").ToListAsync();
            Assert.Single(mods1);
            Assert.Equal("3", mods[0].Id);
        }

        [Fact]
        public async Task ToList_Id_Not_Eq_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").ToListAsync();
            Assert.Equal(999, mods.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
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
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "1").FirstOrDefaultAsync();
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
            var ids = new[] {"200", "300"};
            var dateOfMonths = new[] {202111, 202110};
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
        public async Task OrderCountTest()
        {
            var asyncCount = await _virtualDbContext.Set<Order>().CountAsync();
            Assert.Equal(320, asyncCount);
            var syncCount =  _virtualDbContext.Set<Order>().Count();
            Assert.Equal(320, syncCount);

            var countA =await _virtualDbContext.Set<Order>().CountAsync(o=>o.Area=="A");
            var countB =await _virtualDbContext.Set<Order>().CountAsync(o=>o.Area=="B");
            var countC =await _virtualDbContext.Set<Order>().CountAsync(o=>o.Area=="C");
            Assert.Equal(320, countA+ countB+ countC);
            var fourBegin = new DateTime(2021, 4, 1).Date;
            var fiveBegin = new DateTime(2021, 5, 1).Date;
            var fourCount = await _virtualDbContext.Set<Order>().Where(o=>o.CreateTime>=fourBegin&&o.CreateTime<fiveBegin).CountAsync();
            Assert.Equal(30,fourCount);
        }
        [Fact]
        public async Task OrderFirstTest()
        {
            var threeMonth = new DateTime(2021,3,1);
            var order = await _virtualDbContext.Set<Order>().FirstOrDefaultAsync(o=>o.CreateTime== threeMonth);//第59条 1月31天2月28天
            Assert.NotNull(order);
            Assert.Equal(59,order.Money);
            Assert.Equal("C",order.Area);
        }
        [Fact]
        public async Task OrderOrderTest()
        {
            var orders = await _virtualDbContext.Set<Order>().OrderBy(o => o.CreateTime).ToListAsync();
            Assert.Equal(320,orders.Count);
            var i = 0;
            foreach (var order in orders)
            {
                Assert.Equal(i,order.Money);
                i++;
            }

            var threeMonth = new DateTime(2021, 3, 1);
            var orderPage = await _virtualDbContext.Set<Order>().Where(o=>o.CreateTime > threeMonth).OrderByDescending(o => o.CreateTime).ToShardingPageAsync(1,20);
            Assert.Equal(20, orderPage.Data.Count);
            Assert.Equal(260,orderPage.Total);

            var j = 319;
            foreach (var order in orderPage.Data)
            {
                Assert.Equal(j, order.Money);
                j--;
            }
        }

        [Fact]
        public async Task LogDayCountTest()
        {
            var countAsync = await _virtualDbContext.Set<LogDay>().CountAsync();
            Assert.Equal(3000,countAsync);
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
            var virtualTable = _virtualTableManager.GetVirtualTable(typeof(LogDay));
            var virtualTableName = virtualTable.GetVirtualTableName();
            Assert.Equal(nameof(LogDay),virtualTableName);
            var table = _virtualTableManager.GetVirtualTable(virtualTableName);
            var tryGetVirtualTable = _virtualTableManager.TryGetVirtualTable(typeof(LogDay));
            Assert.NotNull(tryGetVirtualTable);
            var tryGetVirtualTable1 = _virtualTableManager.TryGetVirtualTable(virtualTableName);
            Assert.NotNull(tryGetVirtualTable1);

            var all = virtualTable.GetAllPhysicTables().All(o=>string.IsNullOrWhiteSpace(o.TableSeparator));
            Assert.True(all);
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
           var page= await _virtualDbContext.Set<LogDay>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o=>o.LogTime)
                .ToShardingPageAsync(2, 10);
           Assert.Equal(10, page.Data.Count);
           Assert.Equal(300, page.Total);

           var page1 =  _virtualDbContext.Set<LogDay>().Where(o => o.LogTime >= fourBegin && o.LogTime <= fiveBegin).OrderBy(o => o.LogTime)
               .ToShardingPage(2, 10);
           Assert.Equal(10, page1.Data.Count);
           Assert.Equal(300, page1.Total);

        }
        // [Fact]
        // public async Task Group_API_Test()
        // {
        //     var ids = new[] {"200", "300"};
        //     var dateOfMonths = new[] {202111, 202110};
        //     var group = await _virtualDbContext.Set<SysUserSalary>()
        //         .Where(o => ids.Contains(o.UserId) && dateOfMonths.Contains(o.DateOfMonth))
        //         .ShardingGroupByAsync(g => new {UId = g.UserId}, g => new
        //         {
        //
        //             GroupUserId = g.Key.UId,
        //             Count = g.Count(),
        //             TotalSalary = g.Sum(o => o.Salary),
        //             AvgSalary = g.Average(o => o.Salary),
        //             AvgSalaryDecimal = g.Average(o => o.SalaryDecimal),
        //             MinSalary = g.Min(o => o.Salary),
        //             MaxSalary = g.Max(o => o.Salary)
        //         });
        //     Assert.Equal(2, group.Count);
        //     Assert.Equal(2, group[0].Count);
        //     Assert.Equal(2260000, group[0].TotalSalary);
        //     Assert.Equal(1130000, group[0].AvgSalary);
        //     Assert.Equal(11300, group[0].AvgSalaryDecimal);
        //     Assert.Equal(1120000, group[0].MinSalary);
        //     Assert.Equal(1140000, group[0].MaxSalary);
        // }
    }
}