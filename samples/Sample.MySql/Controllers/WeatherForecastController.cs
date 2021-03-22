using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.MySql.Domain.Entities;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;

namespace Sample.MySql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IVirtualDbContext _virtualDbContext;

        public WeatherForecastController(IVirtualDbContext virtualDbContext)
        {
            _virtualDbContext = virtualDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _virtualDbContext.Set<SysTest>().AnyAsync();
            var result1 = await _virtualDbContext.Set<SysUserMod>().Where(o => o.Id == "2" || o.Id == "3").ToShardingListAsync();
            var shardingCountAsync = await _virtualDbContext.Set<SysUserMod>().ShardingCountAsync();
            var shardingCountAsyn2c = await _virtualDbContext.Set<SysUserLogByMonth>().ShardingCountAsync();

            return Ok(result1);
        }
    }
}
