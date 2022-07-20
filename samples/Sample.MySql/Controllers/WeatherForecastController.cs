using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sample.MySql.DbContexts;
using Sample.MySql.Domain.Entities;
using ShardingCore.Extensions.ShardingQueryableExtensions;
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
            // using (var tran = _defaultTableDbContext.Database.BeginTransaction())
            // {
                
                var resultX = await _defaultTableDbContext.Set<SysUserMod>()
                    .Where(o => o.Id == "2" || o.Id == "3").FirstOrDefaultAsync();
                var resultY = await _defaultTableDbContext.Set<SysUserMod>().FirstOrDefaultAsync(o => o.Id == "2" || o.Id == "3");
                var shardingFirstOrDefaultAsyncxxx = await _defaultTableDbContext.Set<SysUserLogByMonth>().Where(o=>o.Time==DateTime.Now).ToListAsync();
                var result = await _defaultTableDbContext.Set<SysTest>().AnyAsync();
                var result22 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "2"&&o.Name=="ds1").ToListAsync();
                var result1 = await _defaultTableDbContext.Set<SysUserMod>().Where(o => o.Id == "2" || o.Id == "3").ToListAsync();
                var result2 = await _defaultTableDbContext.Set<SysUserLogByMonth>().Skip(1).Take(10).ToListAsync();
                var shardingFirstOrDefaultAsync = await _defaultTableDbContext.Set<SysUserLogByMonth>().ToListAsync();
                var shardingCountAsync = await _defaultTableDbContext.Set<SysUserMod>().CountAsync();
                var shardingCountAsyn2c =  _defaultTableDbContext.Set<SysUserLogByMonth>().Count();
                // var dbConnection = _defaultTableDbContext.Database.GetDbConnection();
                // if (dbConnection.State != ConnectionState.Open)
                // {
                //     dbConnection.Open();
                // }
                // using (var dbCommand = dbConnection.CreateCommand())
                // {
                //     dbCommand.CommandText = "select * from systest";
                //     dbCommand.Transaction = _defaultTableDbContext.Database.CurrentTransaction?.GetDbTransaction();
                //     var dbDataReader = dbCommand.ExecuteReader();
                //     while (dbDataReader.Read())
                //     {
                //         Console.WriteLine(dbDataReader[0]);
                //     }
                // }
            // }
            
            return Ok(1);
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
