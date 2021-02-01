using System.Linq;
using System.Threading.Tasks;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;
using ShardingCore.Test50.Domain.Entities;
using Xunit;

namespace ShardingCore.Test50
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 15 January 2021 17:22:10
* @Email: 326308290@qq.com
*/
    public class ShardingTest
    {
        private readonly IVirtualDbContext _virtualDbContext;

        public ShardingTest(IVirtualDbContext virtualDbContext)
        {
            _virtualDbContext = virtualDbContext;
        }

        [Fact]
        public async Task ToList_All_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().ToShardingListAsync();
            Assert.Equal(1000, mods.Count);
        }

        [Fact]
        public async Task ToList_OrderBy_Asc_Desc_Test()
        {
            var modascs = await _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToShardingListAsync();
            Assert.Equal(1000, modascs.Count);
            var i = 1;
            foreach (var age in modascs)
            {
                Assert.Equal(i, age.Age);
                i++;
            }

            var moddescs = await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToShardingListAsync();
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
            var sysUserMods = await _virtualDbContext.Set<SysUserMod>().Where(o => ids.Contains(o.Id)).ToShardingListAsync();
            foreach (var id in ids)
            {
                Assert.Contains(sysUserMods, o => o.Id == id);
            }

            Assert.DoesNotContain(sysUserMods, o => o.Age > 4);
        }

        [Fact]
        public async Task ToList_Id_Eq_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "3").ToShardingListAsync();
            Assert.Single(mods);
            Assert.Equal("3", mods[0].Id);
        }

        [Fact]
        public async Task ToList_Id_Not_Eq_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").ToShardingListAsync();
            Assert.Equal(999, mods.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
        }

        [Fact]
        public async Task ToList_Id_Not_Eq_Skip_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").OrderBy(o => o.Age).Skip(2).ToShardingListAsync();
            Assert.Equal(997, mods.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
            Assert.Equal(4, mods[0].Age);
            Assert.Equal(5, mods[1].Age);

            var modsDesc = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "3").OrderByDescending(o => o.Age).Skip(13).ToShardingListAsync();
            Assert.Equal(986, modsDesc.Count);
            Assert.DoesNotContain(mods, o => o.Id == "3");
            Assert.Equal(987, modsDesc[0].Age);
            Assert.Equal(986, modsDesc[1].Age);
        }

        [Fact]
        public async Task ToList_Name_Eq_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_3").ToShardingListAsync();
            Assert.Single(mods);
            Assert.Equal("3", mods[0].Id);
        }

        [Fact]
        public async Task ToList_Id_Eq_Not_In_Db_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "1001").ToShardingListAsync();
            Assert.Empty(mods);
        }

        [Fact]
        public async Task ToList_Name_Eq_Not_In_Db_Test()
        {
            var mods = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1001").ToShardingListAsync();
            Assert.Empty(mods);
        }

        [Fact]
        public async Task FirstOrDefault_Order_By_Id_Test()
        {
            var sysUserModAge = await _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserModAge != null && sysUserModAge.Id == "1");
            var sysUserModAgeDesc = await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserModAgeDesc != null && sysUserModAgeDesc.Id == "1000");
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().OrderBy(o => o.Id).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserMod != null && sysUserMod.Id == "1");

            var sysUserModDesc = await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o => o.Id).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserModDesc != null && sysUserModDesc.Id == "999");
        }

        [Fact]
        public async Task FirstOrDefault2()
        {
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "1").ShardingFirstOrDefaultAsync();
            Assert.NotNull(sysUserMod);
            Assert.True(sysUserMod.Id == "1");
        }

        [Fact]
        public async Task FirstOrDefault3()
        {
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_2").ShardingFirstOrDefaultAsync();
            Assert.NotNull(sysUserMod);
            Assert.Equal("2", sysUserMod.Id);
        }

        [Fact]
        public async Task FirstOrDefault4()
        {
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id != "1").ShardingFirstOrDefaultAsync();
            Assert.NotNull(sysUserMod);
            Assert.True(sysUserMod.Id != "1");
        }

        [Fact]
        public async Task FirstOrDefault5()
        {
            var sysUserMod = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1001").ShardingFirstOrDefaultAsync();
            Assert.Null(sysUserMod);
        }

        [Fact]
        public async Task Count_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name == "name_1000").ShardingCountAsync();
            Assert.Equal(1, a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").ShardingCountAsync();
            Assert.Equal(999, b);
        }

        [Fact]
        public async Task Sum_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().ShardingSumAsync(o => o.Age);
            var expected = 0;
            for (int i = 1; i <= 1000; i++)
            {
                expected += i;
            }

            Assert.Equal(expected, a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").ShardingSumAsync(o => o.Age);
            Assert.Equal(expected - 1000, b);
        }

        [Fact]
        public async Task Max_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().ShardingMaxAsync(o => o.Age);
            Assert.Equal(1000, a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1000").ShardingMaxAsync(o => o.Age);
            Assert.Equal(999, b);
            var c = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age < 500).ShardingMaxAsync(o => o.Age);
            Assert.Equal(499, c);
            var e = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age <= 500).ShardingMaxAsync(o => o.Age);
            Assert.Equal(500, e);
        }

        [Fact]
        public async Task Min_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().ShardingMinAsync(o => o.Age);
            Assert.Equal(1, a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1").ShardingMinAsync(o => o.Age);
            Assert.Equal(2, b);
            var c = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age > 500).ShardingMinAsync(o => o.Age);
            Assert.Equal(501, c);
            var e = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age >= 500).ShardingMinAsync(o => o.Age);
            Assert.Equal(500, e);
        }

        [Fact]
        public async Task Any_Test()
        {
            var a = await _virtualDbContext.Set<SysUserMod>().ShardingAnyAsync(o => o.Age == 100);
            Assert.True(a);
            var b = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Name != "name_1").ShardingAnyAsync(o => o.Age == 1);
            Assert.False(b);
            var c = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age > 500).ShardingAnyAsync(o => o.Age <= 500);
            Assert.False(c);
            var e = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Age >= 500).ShardingAnyAsync(o => o.Age <= 500);
            Assert.True(e);
        }

        // [Fact]
        // public async Task Group_Test()
        // {
        //     var x = await (from u in _virtualDbContext.Set<SysUserMod>()
        //         group u by u.AgeGroup
        //         into g
        //         select new
        //         {
        //             AgeGroup = g.Key,
        //             Count = g.Count()
        //         }).ToShardingListAsync();
        //     var y = x;
        // }
    }
}