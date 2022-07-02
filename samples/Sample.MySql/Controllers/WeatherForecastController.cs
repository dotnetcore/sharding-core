using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.MySql.DbContexts;
using Sample.MySql.Domain.Entities;
using ShardingCore.TableCreator;

namespace Sample.MySql.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly DefaultShardingDbContext _defaultTableDbContext;

        public WeatherForecastController(DefaultShardingDbContext defaultTableDbContext)
        {
            _defaultTableDbContext = defaultTableDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _defaultTableDbContext.Set<SysTest>().AnyAsync();
            var result1 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "2" || o.Id == "3").ToListAsync();
            var result2 = await _defaultTableDbContext.Set<SysUserLogByMonth>().Skip(1).Take(10).ToListAsync();
            var shardingFirstOrDefaultAsync = await _defaultTableDbContext.Set<SysUserLogByMonth>().ToListAsync();
            var shardingCountAsync = await _defaultTableDbContext.Set<SysUserMod>().CountAsync();
            var shardingCountAsyn2c =  _defaultTableDbContext.Set<SysUserLogByMonth>().Count();

            return Ok(result1);
        }
        // [HttpGet]
        // public async Task<IActionResult> Get1()
        // {
        //     var allVirtualTables = _virtualTableManager.GetAllVirtualTables();
        //     foreach (var virtualTable in allVirtualTables)
        //     {
        //         if (virtualTable.EntityType == typeof(SysUserLogByMonth))
        //         {
        //             var now = DateTime.Now.Date.AddMonths(2);
        //             var tail = virtualTable.GetVirtualRoute().ShardingKeyToTail(now);
        //             try
        //             {
        //                 _virtualTableManager.AddPhysicTable(virtualTable, new DefaultPhysicTable(virtualTable, tail));
        //                 _tableCreator.CreateTable<SysUserLogByMonth>(tail);
        //             }
        //             catch (Exception e)
        //             {
        //                 //ignore
        //                 Console.WriteLine(e);
        //             }
        //         }
        //     }
        //     return Ok();
        // }
    }
}
