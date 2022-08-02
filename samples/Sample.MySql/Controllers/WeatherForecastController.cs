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
using Sample.MySql.multi;
using ShardingCore.Extensions.ShardingQueryableExtensions;
using ShardingCore.TableCreator;

namespace Sample.MySql.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly DefaultShardingDbContext _defaultTableDbContext;
        private readonly OtherDbContext _otherDbContext;

        public WeatherForecastController(DefaultShardingDbContext defaultTableDbContext,OtherDbContext otherDbContext)
        {
            _defaultTableDbContext = defaultTableDbContext;
            _otherDbContext = otherDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            OtherDbContext.CurrentId = "";
            var myUsers0 = _otherDbContext.MyUsers.ToList();
            OtherDbContext.CurrentId = "123";
            var myUsers1 = _otherDbContext.MyUsers.ToList();
            OtherDbContext.CurrentId = "456";
            var myUsers2= _otherDbContext.MyUsers.ToList();
            
            // var sysUserModQueryable = _otherDbContext.MyUsers.Where(o => o.Id == "2");
            // var dbSetDiscoverExpressionVisitor = new DbSetDiscoverExpressionVisitor<MyUser>(_otherDbContext);
            // dbSetDiscoverExpressionVisitor.Visit(sysUserModQueryable.Expression);
            // var myUsers = dbSetDiscoverExpressionVisitor.DbSet;
            // Console.WriteLine("------------");
            // using (var tran = _defaultTableDbContext.Database.BeginTransaction())
            // {
            var sysUserMods = _defaultTableDbContext.Set<SysUserMod>();
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
                var count = _otherDbContext.Set<MyUser>().Count();
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
        [HttpGet]
        public async Task<IActionResult> Get1()
        {
            var resultX = await _defaultTableDbContext.Set<SysUserMod>()
                .Where(o => o.Id == "2" || o.Id == "3").FirstOrDefaultAsync();
            Console.WriteLine("-----------------------------------------------------------------------------------------------------");
            var resultY = await _defaultTableDbContext.Set<SysUserMod>().FirstOrDefaultAsync(o => o.Id == "2" || o.Id == "3");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------");
            var result = await _defaultTableDbContext.Set<SysTest>().AnyAsync();
            Console.WriteLine("-----------------------------------------------------------------------------------------------------");
            return Ok();
        }
    }
}
