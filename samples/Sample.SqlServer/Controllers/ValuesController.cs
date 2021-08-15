using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sample.SqlServer.DbContexts;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;

namespace Sample.SqlServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ValuesController : ControllerBase
    {

        private readonly DefaultShardingDbContext _defaultTableDbContext;

        public ValuesController(DefaultShardingDbContext defaultTableDbContext)
        {
            _defaultTableDbContext = defaultTableDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _defaultTableDbContext.Set<SysUserMod>().OrderBy(o=>o.Age).ToListAsync();
            return Ok(result);
        }
    }
}