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

        private readonly DefaultTableDbContext _defaultTableDbContext;

        public ValuesController(DefaultTableDbContext defaultTableDbContext)
        {
            _defaultTableDbContext = defaultTableDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _defaultTableDbContext.Set<SysTest>().AnyAsync();
            var result1 = await _defaultTableDbContext.Set<SysUserMod>().Where(o=>o.Id=="2"||o.Id=="3").ToShardingListAsync();
            return Ok(result1);
        }
    }
}