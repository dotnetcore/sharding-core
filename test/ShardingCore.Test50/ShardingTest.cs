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
            var mods=await _virtualDbContext.Set<SysUserMod>().ToShardingListAsync();
            Assert.Equal(100,mods.Count);
            var ranges=await _virtualDbContext.Set<SysUserRange>().ToShardingListAsync();
            Assert.Equal(1000,ranges.Count);
        }
        [Fact]
        public async Task ToList_OrderBy_Asc_Desc_Test()
        {
            var modascs=await _virtualDbContext.Set<SysUserMod>().OrderBy(o=>o.Age).ToShardingListAsync();
            Assert.Equal(100,modascs.Count);
            var i = 1;
            foreach (var age in modascs)
            {
                Assert.Equal(i,age.Age);
                i++;

            }
            var moddescs=await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o=>o.Age).ToShardingListAsync();
            Assert.Equal(100,moddescs.Count);
            var j = 100;
            foreach (var age in moddescs)
            {
                Assert.Equal(j,age.Age);
                j--;

            }
        }
        [Fact]
        public async Task ToList_Id_In_Test()
        {
            var ids = new[] {"1", "2", "3", "4"};
            var sysUserMods=await _virtualDbContext.Set<SysUserMod>().Where(o=>ids.Contains(o.Id)).ToShardingListAsync();
            var sysUserRanges=await _virtualDbContext.Set<SysUserRange>().Where(o=>ids.Contains(o.Id)).ToShardingListAsync();
            foreach (var id in ids)
            {
                Assert.Contains(sysUserMods, o =>o.Id==id);
                Assert.Contains(sysUserRanges, o =>o.Id==id);
            }
            Assert.DoesNotContain(sysUserMods,o=>o.Age>4);
            Assert.DoesNotContain(sysUserRanges,o=>o.Age>4);
        }
        [Fact]
        public async Task ToList_Id_Eq_Test()
        {
            var mods=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id=="3").ToShardingListAsync();
            Assert.Single(mods);
            Assert.Equal("3",mods[0].Id);
            var ranges=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Id=="3").ToShardingListAsync();
            Assert.Single(ranges);
            Assert.Equal("3",ranges[0].Id);
        }
        [Fact]
        public async Task ToList_Id_Not_Eq_Test()
        {
            var mods=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id!="3").ToShardingListAsync();
            Assert.Equal(99,mods.Count);
            Assert.DoesNotContain(mods,o=>o.Id=="3");
            var ranges=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Id!="3").ToShardingListAsync();
            Assert.Equal(999,ranges.Count);
            Assert.DoesNotContain(ranges,o=>o.Id=="3");
        }
        [Fact]
        public async Task ToList_Id_Not_Eq_Skip_Test()
        {
            var mods=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id!="3").OrderBy(o=>o.Age).Skip(2).ToShardingListAsync();
            Assert.Equal(97,mods.Count);
            Assert.DoesNotContain(mods,o=>o.Id=="3");
            Assert.Equal(4,mods[0].Age);
            Assert.Equal(5,mods[1].Age);
            
            var modsDesc=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id!="3").OrderByDescending(o=>o.Age).Skip(13).ToShardingListAsync();
            Assert.Equal(86,modsDesc.Count);
            Assert.DoesNotContain(mods,o=>o.Id=="3");
            Assert.Equal(87,modsDesc[0].Age);
            Assert.Equal(86,modsDesc[1].Age);
        }
        [Fact]
        public async Task ToList_Name_Eq_Test()
        {
            var mods=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Name=="name_3").ToShardingListAsync();
            Assert.Single(mods);
            Assert.Equal("3",mods[0].Id);
            var ranges=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Name=="name_range_3").ToShardingListAsync();
            Assert.Single(ranges);
            Assert.Equal("3",ranges[0].Id);
        }
        [Fact]
        public async Task ToList_Id_Eq_Not_In_Db_Test()
        {
            var mods=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id=="1001").ToShardingListAsync();
            Assert.Empty(mods);
            var ranges=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Id=="1001").ToShardingListAsync();
            Assert.Empty(ranges);
        }
        [Fact]
        public async Task ToList_Name_Eq_Not_In_Db_Test()
        {
            var mods=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Name=="name_1001").ToShardingListAsync();
            Assert.Empty(mods);
            var ranges=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Name=="name_range_1001").ToShardingListAsync();
            Assert.Empty(ranges);
        }
        [Fact]
        public async Task FirstOrDefault_Order_By_Id_Test()
        {
            var sysUserModAge=await _virtualDbContext.Set<SysUserMod>().OrderBy(o=>o.Age).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserModAge!=null&&sysUserModAge.Id=="1");
            var sysUserModAgeDesc=await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o=>o.Age).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserModAgeDesc!=null&&sysUserModAgeDesc.Id=="100");
            var sysUserMod=await _virtualDbContext.Set<SysUserMod>().OrderBy(o=>o.Id).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserMod!=null&&sysUserMod.Id=="1");
            
            var sysUserModDesc=await _virtualDbContext.Set<SysUserMod>().OrderByDescending(o=>o.Id).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserModDesc!=null&&sysUserModDesc.Id=="99");
            var sysUserRange=await _virtualDbContext.Set<SysUserRange>().OrderBy(o=>o.Id).ShardingFirstOrDefaultAsync();
            Assert.True(sysUserRange!=null&&sysUserRange.Id=="1");
        }
        [Fact]
        public async Task FirstOrDefault2()
        {
            var sysUserMod=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id=="1").ShardingFirstOrDefaultAsync();
            Assert.True(sysUserMod!=null&&sysUserMod.Id=="1");
            var sysUserRange=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Id=="1").ShardingFirstOrDefaultAsync();
            Assert.True(sysUserRange!=null&&sysUserRange.Id=="1");
        }
        [Fact]
        public async Task FirstOrDefault3()
        {
            var sysUserMod=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Name=="name_2").ShardingFirstOrDefaultAsync();
            Assert.NotNull(sysUserMod);
            Assert.Equal("2",sysUserMod.Id);
            var sysUserRange=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Name=="name_range_2").ShardingFirstOrDefaultAsync();
            Assert.NotNull(sysUserRange);
            Assert.Equal("2",sysUserRange.Id);
        }
        [Fact]
        public async Task FirstOrDefault4()
        {
            var sysUserMod=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id!="1").ShardingFirstOrDefaultAsync();
            Assert.NotNull(sysUserMod);
            Assert.True(sysUserMod.Id!="1");
            var sysUserRange=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Id!="1").ShardingFirstOrDefaultAsync();
            Assert.NotNull(sysUserRange);
            Assert.True(sysUserRange.Id!="1");
        }
        [Fact]
        public async Task FirstOrDefault5()
        {
            var sysUserMod=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Name=="name_101").ShardingFirstOrDefaultAsync();
            Assert.Null(sysUserMod);
            var sysUserRange=await _virtualDbContext.Set<SysUserRange>().Where(o=>o.Name=="name_range_1001").ShardingFirstOrDefaultAsync();
            Assert.Null(sysUserRange);
        }
        [Fact]
        public async Task Count_Test()
        {
            var a=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Name=="name_100").ShardingCountAsync();
            Assert.Equal(1,a);
            var b=await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Name!="name_100").ShardingCountAsync();
            Assert.Equal(99,b);
        }
    }
}