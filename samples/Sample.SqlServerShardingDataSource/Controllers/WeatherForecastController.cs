using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingDataSource.DbContexts;
using Sample.SqlServerShardingDataSource.Domain.Entities;

namespace Sample.SqlServerShardingDataSource.Controllers
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
        private readonly DefaultShardingDbContext _defaultShardingDbContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DefaultShardingDbContext defaultShardingDbContext)
        {
            _logger = logger;
            _defaultShardingDbContext = defaultShardingDbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var resultx112331 = await _defaultShardingDbContext.Set<SysUserMod>().CountAsync();
            var resultx11233411 = _defaultShardingDbContext.Set<SysUserMod>().Count();
            var resultx11231 = await _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Age == 198198).Select(o => o.Id).ContainsAsync("1981");
            var resultx1121 = await _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").SumAsync(o => o.Age);
            var resultx111 = await _defaultShardingDbContext.Set<SysUserMod>().FirstOrDefaultAsync(o => o.Id == "198");
            var resultx2 = await _defaultShardingDbContext.Set<SysUserMod>().CountAsync(o => o.Age <= 10);
            var resultx = await _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").FirstOrDefaultAsync();
            var resultx33 = await _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Select(o => o.Id).FirstOrDefaultAsync();
            var resulxxt = await _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").ToListAsync();
            var result = await _defaultShardingDbContext.Set<SysUserMod>().ToListAsync();




            var sresultx11231 = _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Age == 198198).Select(o => o.Id).Contains("1981");
            var sresultx1121 = _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Sum(o => o.Age);
            var sresultx111 = _defaultShardingDbContext.Set<SysUserMod>().FirstOrDefault(o => o.Id == "198");
            var sresultx2 = _defaultShardingDbContext.Set<SysUserMod>().Count(o => o.Age <= 10);
            var sresultx = _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").FirstOrDefault();
            var sresultx33 = _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Select(o => o.Id).FirstOrDefault();
            var sresultxc = _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").Select(o => o.Id).ToList();
            var sresultxasdc = _defaultShardingDbContext.Set<SysUserMod>().Where(o => o.Id == "198").ToList();
            var sresult = _defaultShardingDbContext.Set<SysUserMod>().ToList();
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
