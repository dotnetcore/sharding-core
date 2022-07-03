using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.RuntimeContexts;
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
        private readonly IShardingTableCreator _shardingTableCreator;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,DefaultDbContext defaultDbContext, IShardingRuntimeContext shardingRuntimeContext)
        {
            _logger = logger;
            _defaultDbContext = defaultDbContext;
            _shardingTableCreator = shardingRuntimeContext.GetShardingTableCreator();
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Console.WriteLine("---------------开始-----------------");
            var s = DateTime.Now.ToString("HHmmss");
            // Task.Run(() =>
            // {
            //     try
            //     {
            //         var virtualTable = _virtualTableManager.GetVirtualTable(typeof(SysUserMod));
            //         _virtualTableManager.AddPhysicTable(typeof(SysUserMod), new DefaultPhysicTable(virtualTable, s));
            //         _shardingTableCreator.CreateTable<SysUserMod>("A", s);
            //     }
            //     catch (Exception e)
            //     {
            //         Console.WriteLine(e);
            //     }
            // });
            // Task.Run(() =>
            // {
            //     try
            //     {
            //         var virtualTable = _virtualTableManager.GetVirtualTable(typeof(SysUserModAbc));
            //         _virtualTableManager.AddPhysicTable(typeof(SysUserModAbc), new DefaultPhysicTable(virtualTable, s));
            //         _shardingTableCreator.CreateTable<SysUserModAbc>("A", s);
            //     }
            //     catch (Exception e)
            //     {
            //         Console.WriteLine(e);
            //     }
            // });
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
