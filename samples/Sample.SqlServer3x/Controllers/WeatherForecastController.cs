using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServer3x.Domain.Entities;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.TableCreator;

namespace Sample.SqlServer3x.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DefaultDbContext _defaultDbContext;
        private readonly IShardingTableCreator<DefaultDbContext> _shardingTableCreator;
        private readonly IVirtualTableManager<DefaultDbContext> _virtualTableManager;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,DefaultDbContext defaultDbContext, IShardingTableCreator<DefaultDbContext> shardingTableCreator, IVirtualTableManager<DefaultDbContext> virtualTableManager)
        {
            _logger = logger;
            _defaultDbContext = defaultDbContext;
            _shardingTableCreator = shardingTableCreator;
            _virtualTableManager = virtualTableManager;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Console.WriteLine("---------------开始-----------------");
            var s = DateTime.Now.ToString("HHmmss");
            Task.Run(() =>
            {
                try
                {
                    var virtualTable = _virtualTableManager.GetVirtualTable(typeof(SysUserMod));
                    _virtualTableManager.AddPhysicTable(typeof(SysUserMod), new DefaultPhysicTable(virtualTable, s));
                    _shardingTableCreator.CreateTable<SysUserMod>("A", s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            Task.Run(() =>
            {
                try
                {
                    var virtualTable = _virtualTableManager.GetVirtualTable(typeof(SysUserModAbc));
                    _virtualTableManager.AddPhysicTable(typeof(SysUserModAbc), new DefaultPhysicTable(virtualTable, s));
                    _shardingTableCreator.CreateTable<SysUserModAbc>("A", s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            //try
            //{
            //    var virtualTable = _virtualTableManager.GetVirtualTable(typeof(SysUserMod));
            //    _virtualTableManager.AddPhysicTable(typeof(SysUserMod), new DefaultPhysicTable(virtualTable, s));
            //    _shardingTableCreator.CreateTable<SysUserMod>("A", s);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}
            //try
            //{
            //    var virtualTable = _virtualTableManager.GetVirtualTable(typeof(SysUserModAbc));
            //    _virtualTableManager.AddPhysicTable(typeof(SysUserModAbc), new DefaultPhysicTable(virtualTable, s));
            //    _shardingTableCreator.CreateTable<SysUserModAbc>("A", s);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
