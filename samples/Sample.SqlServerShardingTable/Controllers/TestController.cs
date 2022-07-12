using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingTable.Entities;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;

namespace Sample.SqlServerShardingTable.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly MyDbContext _myDbContext;
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public TestController(MyDbContext myDbContext,IShardingRuntimeContext shardingRuntimeContext)
        {
            _myDbContext = myDbContext;
            _shardingRuntimeContext = shardingRuntimeContext;
        }
        public async Task<IActionResult> Testa()
        {
            Stopwatch sp=Stopwatch.StartNew();
            var listAsync = await _myDbContext.Set<SysUser>().AsTracking().ToListAsync();
            sp.Stop();
            return Ok(sp.ElapsedMilliseconds);
        }
        public async Task<IActionResult> Testb()
        {
            Stopwatch sp = Stopwatch.StartNew();
            var listAsync = await _myDbContext.Set<SysUser>().AsNoTracking().ToListAsync();
            sp.Stop();
            return Ok(sp.ElapsedMilliseconds);
        }
        public async Task<IActionResult> Query()
        {
            Console.WriteLine("123123");
            var dateTime = new DateTime(2021,2,1);
            var orderSet = _myDbContext.Set<Order>().Where(o=>o.CreationTime== dateTime);
            var listAsync = await _myDbContext.Set<SysUser>().Where(o=> orderSet.Any(u=>u.Id== o.Id)).ToListAsync();
            //Console.WriteLine("123123456");
            //var orderSet1 = _myDbContext.Set<Order>().Select(o => o);
            //var listAsync2 = await _myDbContext.Set<SysUser>().Where(o => orderSet1.Any(u => u.Id == o.Id)).ToListAsync();

            //Console.WriteLine("456456");
            //var dbSet1 = _myDbContext.Set<Order>();
            //var dbSet2 = _myDbContext.Set<Order>();
            //var queryable = (from u in dbSet1
            //                 join x in dbSet2
            //        on u.Id equals x.Id
            //                 select u
            //        );
            //var @async = queryable.ToListAsync();
            //var sysUser =await _myDbContext.Set<SysUser>().Where(o=>o.Id=="1").FirstOrDefaultAsync();
            //var dateTime = new DateTime(2021,3,5);
            //var order = await _myDbContext.Set<Order>().Where(o=>o.CreationTime>= dateTime).OrderBy(o=>o.CreationTime).FirstOrDefaultAsync();
            //var orderIdOne = await _myDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == "3");

            //var sysUsers = await _myDbContext.Set<SysUser>().Where(o => o.Id == "1" || o.Id=="6").ToListAsync();

            //return Ok(new object[]
            //{
            //    sysUser,
            //    order,
            //    orderIdOne,
            //    sysUsers
            //});
            return Ok();
        }
        public async Task<IActionResult> Query2()
        {
            var multiOrder =await _myDbContext.Set<MultiShardingOrder>().Where(o=>o.Id== 232398109278351360).FirstOrDefaultAsync();
            var multiOrder1 =await _myDbContext.Set<MultiShardingOrder>().IgnoreQueryFilters().Where(o=>o.Id== 232398109278351360).FirstOrDefaultAsync();
            var longs = new []{ 232398109278351360 , 255197859283087360 };
            var multiOrders = await _myDbContext.Set<MultiShardingOrder>().Where(o => longs.Contains(o.Id)).ToListAsync();
            var dateTime = new DateTime(2021, 11, 1);
            var multiOrder404 = await _myDbContext.Set<MultiShardingOrder>().Where(o => o.Id == 250345338962063360&&o.CreateTime< dateTime).FirstOrDefaultAsync();
            return Ok(new
            {
                multiOrder,
                multiOrders,
                multiOrder404
            });

        }
        public async Task<IActionResult> QueryJoin1()
        {
           var sql= from user in _myDbContext.Set<SysUser>().Where(o => o.Id == "1" || o.Id == "6")
                join setting in _myDbContext.Set<Setting>()
                    on user.SettingCode equals setting.Code
                select new
                {
                    user.Id,
                    user.Name,
                    user.Area,
                    user.SettingCode,
                    SettingName=setting.Name,
                };
            return Ok(await sql.ToListAsync());
        }
        public async Task<IActionResult> QueryJoin2()
        {
           var begin = new DateTime(2021, 3, 2);
           var end = new DateTime(2021, 3, 28);
           var sql1 = from user in _myDbContext.Set<SysUser>().Where(o => o.Id == "1")
               join order in _myDbContext.Set<Order>().Where(o=>o.CreationTime>=begin&&o.CreationTime<=end)
                   on user.Id equals order.Payer
               select new
               {
                   user.Id,
                   user.Name,
                   user.Area,
                   user.SettingCode,
                   OrderId = order.Id,
                   order.Payer,
                   order.CreationTime
               };
            return Ok(await sql1.ToListAsync());
        }
        public async Task<IActionResult> Update()
        {
            var sysUser = await _myDbContext.Set<SysUser>().Where(o => o.Id == "1").FirstOrDefaultAsync();
            sysUser.Name = "new name"+new Random().Next(1,1000);
            var i=await _myDbContext.SaveChangesAsync();
            return Ok(i);
        }
        public async Task<IActionResult> Update1()
        {
            var sysUser = await _myDbContext.Set<SysUser>().AsNoTracking().Where(o => o.Id == "1").FirstOrDefaultAsync();
            sysUser.Name = "new name";
            _myDbContext.Update(sysUser);
            var i = await _myDbContext.SaveChangesAsync();
            return Ok(i);
        }
        public async Task<IActionResult> Delete()
        {
            var sysUser = await _myDbContext.Set<SysUser>().Where(o => o.Id == "9").FirstOrDefaultAsync();
            _myDbContext.Remove(sysUser);
            var i = await _myDbContext.SaveChangesAsync();
            return Ok(i);
        }

        public async Task<IActionResult> DynamicReadWrite()
        {
            DynamicShardingHelper.DynamicAppendReadWriteConnectionString(_shardingRuntimeContext,"ds0", "Data Source=localhost;Initial Catalog=EFCoreShardingTableDB1;Integrated Security=True;");
            var sysUser = await _myDbContext.Set<SysUser>().Where(o => o.Id == "1").FirstOrDefaultAsync();

            return Ok(sysUser);
        }
        public async Task<IActionResult> Read()
        {
            _myDbContext.ReadWriteSeparationWriteOnly();
               var sysUser = await _myDbContext.Set<SysUser>().Where(o => o.Id == "1").FirstOrDefaultAsync();
           
            return Ok(sysUser);
        }
    }
}
