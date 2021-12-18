using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.NoShardingMultiLevel.Entities;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails;

namespace Sample.NoShardingMultiLevel.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DefaultDbContext _defaultDbContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DefaultDbContext defaultDbContext)
        {
            _logger = logger;
            _defaultDbContext = defaultDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //var dbContext = _defaultDbContext.GetDbContext("ds0",false,new SingleQueryRouteTail(string.Empty));
            var boss =await _defaultDbContext.Set<Boss>().Include(o=>o.Company).FirstOrDefaultAsync();
            if (boss!=null)
            {
                var companyId = boss.Company.Id;
                boss.Company.Name = "new company";
                boss.Company.Departments = new List<Department>()
                {
                    new Department() { Id = Guid.NewGuid().ToString("n"), Name = "department", CompanyId = companyId }
                };
            }

            await _defaultDbContext.SaveChangesAsync();
            return Ok("ok");
        }
        [HttpGet]
        public async Task<IActionResult> Get1()
        {
            var boss = new Boss() { Id = Guid.NewGuid().ToString("n"), Name = "boss" };
            await _defaultDbContext.AddAsync(boss);
            var company = new Company() { Id = Guid.NewGuid().ToString("n"), Name = "company", BossId = boss.Id };
            await _defaultDbContext.AddAsync(company);
            await _defaultDbContext.SaveChangesAsync();
            return Ok("ok");
        }
    }
}