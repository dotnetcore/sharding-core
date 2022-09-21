using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.MySql.DbContexts;
using Sample.MySql.Domain.Entities;
using Sample.MySql.multi;
using Sample.MySql.Shardings;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

namespace Sample.MySql.Controllers
{
    public class ssss
    {
        public string Id { get; set; }
        public int C { get; set; }
    }
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly DefaultShardingDbContext _defaultTableDbContext;
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public WeatherForecastController(DefaultShardingDbContext defaultTableDbContext,IShardingRuntimeContext shardingRuntimeContext)
        {
            _defaultTableDbContext = defaultTableDbContext;
            _shardingRuntimeContext = shardingRuntimeContext;
        }

        public IQueryable<SysTest> GetAll()
        {
            var shardingTableCreator = _shardingRuntimeContext.GetShardingTableCreator();
            var tableRouteManager = _shardingRuntimeContext.GetTableRouteManager();
            //系统的时间分片都会实现 ITailAppendable 如果不是系统的自定义的转成你自己的对象即可
            var virtualTableRoute = (ITailAppendable)tableRouteManager.GetRoute(typeof(SysUserMod));
            //一定要先在路由里面添加尾巴
            virtualTableRoute.Append("20220921");
            shardingTableCreator.CreateTable<SysUserMod>("ds0","20220921");

            return _defaultTableDbContext.Set<SysTest>();
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
             OtherDbContext.CurrentId = "";
            // var myUsers0 = _otherDbContext.MyUsers.ToList();
            // OtherDbContext.CurrentId = "123";
            // var myUsers1 = _otherDbContext.MyUsers.ToList();
            // OtherDbContext.CurrentId = "456";
            // var myUsers2= _otherDbContext.MyUsers.ToList();
            
            // var sysUserModQueryable = _otherDbContext.MyUsers.Where(o => o.Id == "2");
            // var dbSetDiscoverExpressionVisitor = new DbSetDiscoverExpressionVisitor<MyUser>(_otherDbContext);
            // dbSetDiscoverExpressionVisitor.Visit(sysUserModQueryable.Expression);
            // var myUsers = dbSetDiscoverExpressionVisitor.DbSet;
            // Console.WriteLine("------------");
            // using (var tran = _defaultTableDbContext.Database.BeginTransaction())
            // {
            var sysUserMods = _defaultTableDbContext.Set<SysUserMod>().OrderBy(o=>o.Id).ThenBy(o=>o.Name);

            var sysUserMods1 = _defaultTableDbContext.Set<SysTest>()
                .Select(o => new ssss(){ Id = o.Id, C = GetAll().Count(x => x.Id == o.Id) }).ToList();
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
