using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.MySql.Domain.Entities;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;
using ShardingCore.TableCreator;

namespace Sample.MySql.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IVirtualDbContext _virtualDbContext;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingTableCreator _tableCreator;

        public WeatherForecastController(IVirtualDbContext virtualDbContext,IVirtualTableManager virtualTableManager, IShardingTableCreator tableCreator)
        {
            _virtualDbContext = virtualDbContext;
            _virtualTableManager = virtualTableManager;
            _tableCreator = tableCreator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var taleAllTails = _virtualTableManager.GetVirtualTable("conn1",typeof(SysUserLogByMonth)).GetTaleAllTails();


            var result = await _virtualDbContext.Set<SysTest>().AnyAsync();
            var result1 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "2" || o.Id == "3").ToShardingListAsync();
            var result2 = await _virtualDbContext.Set<SysUserLogByMonth>().Skip(1).Take(10).ToShardingListAsync();
            var shardingFirstOrDefaultAsync = await _virtualDbContext.Set<SysUserLogByMonth>().ShardingFirstOrDefaultAsync();
            var shardingCountAsync = await _virtualDbContext.Set<SysUserMod>().ShardingCountAsync();
            var shardingCountAsyn2c =  _virtualDbContext.Set<SysUserLogByMonth>().ShardingCount();

            return Ok(result1);
        }
        [HttpGet]
        public async Task<IActionResult> Get1()
        {
            var allVirtualTables = _virtualTableManager.GetAllVirtualTables("conn1");
            foreach (var virtualTable in allVirtualTables)
            {
                if (virtualTable.EntityType == typeof(SysUserLogByMonth))
                {
                    var now = DateTime.Now.Date.AddMonths(2);
                    var tail = virtualTable.GetVirtualRoute().ShardingKeyToTail(now);
                    try
                    {
                        _virtualTableManager.AddPhysicTable("conn1", virtualTable, new DefaultPhysicTable(virtualTable, tail));
                        _tableCreator.CreateTable<SysUserLogByMonth>("conn1", tail);
                    }
                    catch (Exception e)
                    {
                        //ignore
                        Console.WriteLine(e);
                    }
                }
            }
            return Ok();
        }
    }
}
