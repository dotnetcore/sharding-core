using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;

namespace Sample.SqlServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ValuesController : ControllerBase
    {

        private readonly IVirtualDbContext _virtualDbContext;

        public ValuesController(IVirtualDbContext virtualDbContext)
        {
            _virtualDbContext = virtualDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _virtualDbContext.Set<SysUserMod>().ToShardingListAsync());
        }
    }
}