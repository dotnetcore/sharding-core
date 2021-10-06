// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Sample.MySql.DbContexts;
// using Sample.MySql.Domain.Entities;
// using ShardingCore.Core.PhysicTables;
// using ShardingCore.Core.VirtualTables;
// using ShardingCore.DbContexts.VirtualDbContexts;
// using ShardingCore.Extensions;
// using ShardingCore.TableCreator;
//
// namespace Sample.MySql.Controllers
// {
//     [ApiController]
//     [Route("[controller]/[action]")]
//     public class WeatherForecastController : ControllerBase
//     {
//
//         private readonly DefaultTableDbContext _defaultTableDbContext;
//         private readonly IVirtualTableManager _virtualTableManager;
//         private readonly IShardingTableCreator _tableCreator;
//
//         public WeatherForecastController(DefaultTableDbContext defaultTableDbContext,IVirtualTableManager virtualTableManager, IShardingTableCreator tableCreator)
//         {
//             _defaultTableDbContext = defaultTableDbContext;
//             _virtualTableManager = virtualTableManager;
//             _tableCreator = tableCreator;
//         }
//
//         [HttpGet]
//         public async Task<IActionResult> Get()
//         {
//             var taleAllTails = _virtualTableManager.GetVirtualTable(typeof(SysUserLogByMonth)).GetTableAllTails();
//
//
//             var result = await _defaultTableDbContext.Set<SysTest>().AnyAsync();
//             var result1 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "2" || o.Id == "3").ToShardingListAsync();
//             var result2 = await _defaultTableDbContext.Set<SysUserLogByMonth>().Skip(1).Take(10).ToShardingListAsync();
//             var shardingFirstOrDefaultAsync = await _defaultTableDbContext.Set<SysUserLogByMonth>().ShardingFirstOrDefaultAsync();
//             var shardingCountAsync = await _defaultTableDbContext.Set<SysUserMod>().ShardingCountAsync();
//             var shardingCountAsyn2c =  _defaultTableDbContext.Set<SysUserLogByMonth>().ShardingCount();
//
//             return Ok(result1);
//         }
//         [HttpGet]
//         public async Task<IActionResult> Get1()
//         {
//             var allVirtualTables = _virtualTableManager.GetAllVirtualTables();
//             foreach (var virtualTable in allVirtualTables)
//             {
//                 if (virtualTable.EntityType == typeof(SysUserLogByMonth))
//                 {
//                     var now = DateTime.Now.Date.AddMonths(2);
//                     var tail = virtualTable.GetVirtualRoute().ShardingKeyToTail(now);
//                     try
//                     {
//                         _virtualTableManager.AddPhysicTable(virtualTable, new DefaultPhysicTable(virtualTable, tail));
//                         _tableCreator.CreateTable<SysUserLogByMonth>(tail);
//                     }
//                     catch (Exception e)
//                     {
//                         //ignore
//                         Console.WriteLine(e);
//                     }
//                 }
//             }
//             return Ok();
//         }
//     }
// }
