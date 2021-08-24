using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Test50_2x.Domain.Entities;
using Xunit;

namespace ShardingCore.Test50_2x
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

        public ShardingTest(ShardingDefaultDbContext virtualDbContext,IShardingRouteManager shardingRouteManager)
        {
            _virtualDbContext = virtualDbContext;
            _shardingRouteManager = shardingRouteManager;
        }

        //[Fact]
        //public async Task Route_TEST()
        //{
        //    var queryable1 = _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id=="339");
        //    var routeResults1 = _routingRuleEngineFactory.Route(queryable1);
        //    Assert.Equal(1,routeResults1.Count());
        //    Assert.Equal(1,routeResults1.FirstOrDefault().ReplaceTables.Count());
        //    Assert.Equal("0",routeResults1.FirstOrDefault().ReplaceTables.FirstOrDefault().Tail);
        //    Assert.Equal(nameof(SysUserMod),routeResults1.FirstOrDefault().ReplaceTables.FirstOrDefault().OriginalName);
        //    var ids = new[] {"339", "124","142"};
        //    var queryable2= _virtualDbContext.Set<SysUserMod>().Where(o=>ids.Contains(o.Id));
        //    var routeResult2s = _routingRuleEngineFactory.Route(queryable2);
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
                _shardingRouteManager.Current.Must.TryAdd(typeof(SysUserMod), new HashSet<string>() { "00" });

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
            var sysUserMods = await _virtualDbContext.Set<SysUserMod>().Where(o => ids.Contains(o.Id)).ToListAsync();
            foreach (var id in ids)
            {
                Assert.Contains(sysUserMods, o => o.Id == id);
            }

            Assert.DoesNotContain(sysUserMods, o => o.Age > 4);
        }

        [Fact]
        public async Task ToList_Id_Eq_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "3").ToListAsync();
            Assert.Single(mods);
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
            Assert.True(sysUserMod.Id == "1");
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