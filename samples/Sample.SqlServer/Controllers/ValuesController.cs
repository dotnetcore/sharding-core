using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Sample.SqlServer.DbContexts;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;
using ShardingCore.Extensions.ShardingPageExtensions;

namespace Sample.SqlServer.Controllers
{
    public class STU
    {
        public string Id { get; set; }
    }
    [ApiController]
    [Route("[controller]/[action]")]
    public class ValuesController : ControllerBase
    {

        private readonly DefaultShardingDbContext _defaultTableDbContext;
        private readonly IShardingRouteManager _shardingRouteManager;

        public ValuesController(DefaultShardingDbContext defaultTableDbContext, IShardingRouteManager shardingRouteManager)
        {
            _defaultTableDbContext = defaultTableDbContext;
            _shardingRouteManager = shardingRouteManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var sql = from ut in _defaultTableDbContext.Set<SysTest>()
                      join u in _defaultTableDbContext.Set<SysUserMod>()
                          on ut.UserId equals u.Id
                      select new
                      {
                          ut.Id,
                          userId = u.Id
                      };
            var listAsync = await sql.ToListAsync();
            var resultx112331tt = await _defaultTableDbContext.Set<SysTest>().CountAsync();
            var resultx112331 = await _defaultTableDbContext.Set<SysUserMod>().CountAsync();
            var resultx11233411 = _defaultTableDbContext.Set<SysUserMod>().Count();
            var resultx11231 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Age == 198198).Select(o => o.Id).ContainsAsync("1981");
            var resultx1121 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").SumAsync(o => o.Age);
            var resultx111 = await _defaultTableDbContext.Set<SysUserMod>().FirstOrDefaultAsync(o => o.Id == "198");
            var resultx2 = await _defaultTableDbContext.Set<SysUserMod>().CountAsync(o => o.Age <= 10);
            var resultx = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").FirstOrDefaultAsync();
            var resultx33 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Select(o => o.Id).FirstOrDefaultAsync();
            var resulxxt = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").ToListAsync();
            var result = await _defaultTableDbContext.Set<SysUserMod>().ToListAsync();




            var sresultx11231 = _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Age == 198198).Select(o => o.Id).Contains("1981");
            var sresultx1121 = _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Sum(o => o.Age);
            var sresultx111 = _defaultTableDbContext.Set<SysUserMod>().FirstOrDefault(o => o.Id == "198");
            var sresultx2 = _defaultTableDbContext.Set<SysUserMod>().Count(o => o.Age <= 10);
            var sresultx = _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").FirstOrDefault();
            var sresultx33 = _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Select(o => o.Id).FirstOrDefault();
            var sresultxc = _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Select(o => o.Id).ToList();
            var sresultxasdc = _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "198").ToList();
            var sresult = _defaultTableDbContext.Set<SysUserMod>().ToList();
            var sysUserMod98 = result.FirstOrDefault(o => o.Id == "98");
            sysUserMod98.Name = "name_update" + new Random().Next(1, 99) + "_98";
            using (var tran = _defaultTableDbContext.Database.BeginTransaction())
            {
                await _defaultTableDbContext.SaveChangesAsync();
                tran.Commit();
            }

            var stu = new STU() { Id = "198" };
            var sresultx111x = _defaultTableDbContext.Set<SysUserMod>().FirstOrDefault(o => o.Id == stu.Id);

            var pageResult = await _defaultTableDbContext.Set<SysUserMod>().Skip(10).Take(10).OrderBy(o => o.Age).ToListAsync();

            var resuxaasa = await _defaultTableDbContext.Set<SysTest>().ToListAsync();

            using (_shardingRouteManager.CreateScope())
            {
                _shardingRouteManager.Current.TryCreateOrAddMustTail<SysUserMod>("00");

                var mod00s = await _defaultTableDbContext.Set<SysUserMod>().Skip(10).Take(11).ToListAsync();
            }
            //_defaultTableDbContext.RemoveRange(_defaultTableDbContext.Set<SysUserMod>());
            //await _defaultTableDbContext.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> Get1([FromQuery] int p, [FromQuery] int s)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var shardingPageResultAsync = await _defaultTableDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToShardingPageAsync(p, s);
            sp.Stop();
            return Ok(new
            {
                sp.ElapsedMilliseconds,
                shardingPageResultAsync
            });
        }
        [HttpGet]
        public IActionResult Get2([FromQuery] int p, [FromQuery] int s)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var shardingPageResultAsync = _defaultTableDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToShardingPage(p, s);
            sp.Stop();
            return Ok(new
            {
                sp.ElapsedMilliseconds,
                shardingPageResultAsync
            });
        }
        //[HttpGet]
        //public IActionResult Get3()
        //{

        //    var dbContext2s = _defaultTableDbContext.BulkShardingExpression<SysUserMod>(o => o.Age > 100);
        //    using (var tran = _defaultTableDbContext.Database.BeginTransaction())
        //    {
        //        dbContext2s.ForEach(dbContext =>
        //        {
        //            dbContext.Set<SysUserMod>().Where(o => o.Age > 100).Update(o => new SysUserMod()
        //            {
        //                AgeGroup = 1000
        //            });
        //        });
        //        _defaultTableDbContext.SaveChanges();
        //        tran.Commit();
        //    }
        //    var list = new List<SysUserMod>();
        //    var dbContexts = _defaultTableDbContext.BulkShardingEnumerable(list);

        //    using (var tran = _defaultTableDbContext.Database.BeginTransaction())
        //    {
        //        dbContexts.ForEach(kv =>
        //        {
        //            kv.Key.BulkInsert(kv.Value);
        //        });
        //        dbContexts.ForEach(kv =>
        //        {
        //            kv.Key.BulkDelete(kv.Value);
        //        });
        //        dbContexts.ForEach(kv =>
        //        {
        //            kv.Key.BulkUpdate(kv.Value);
        //        });
        //        _defaultTableDbContext.SaveChanges();
        //        tran.Commit();
        //    }


        //    return Ok();
        //}

    }
}
