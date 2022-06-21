using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingDataSource.Entities;
using ShardingCore.Helpers;

namespace Sample.SqlServerShardingDataSource.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly MyDbContext _myDbContext;

        public TestController(MyDbContext myDbContext)
        {
            _myDbContext = myDbContext;
        }
        /// <summary>
        /// 动态追加分库
        /// </summary>
        /// <returns></returns>
        //public IActionResult Queryabc()
        //{
        //    DynamicShardingHelper.DynamicAppendDataSource();
        //    DbContextHelper.CreateSubDb("X", "Data Source=localhost;Initial Catalog=EFCoreShardingDataSourceDBX;Integrated Security=True;");
        //    return Ok();
        //}
        public async Task<IActionResult> Query()
        {
            var sysUser =await _myDbContext.Set<SysUser>().Where(o=>o.Id=="1").FirstOrDefaultAsync();
            var dateTime = new DateTime(2021,3,5);
            var order = await _myDbContext.Set<Order>().Where(o=>o.CreationTime>= dateTime).OrderBy(o=>o.CreationTime).FirstOrDefaultAsync();
            var orderIdOne = await _myDbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == "3");


            var sysUsers = await _myDbContext.Set<SysUser>().Where(o => o.Id == "1" || o.Id=="6").ToListAsync();

            return Ok(new object[]
            {
                sysUser,
                order,
                orderIdOne,
                sysUsers
            });
        }
        public async Task<IActionResult> QueryJoin()
        {
           var begin = new DateTime(2021, 3, 2);
           var end = new DateTime(2021, 4, 3);
           var sql1 = from user in _myDbContext.Set<SysUser>().Where(o => o.Id == "1" || o.Id == "6")
               join order in _myDbContext.Set<Order>().Where(o=>o.CreationTime>=begin&&o.CreationTime<=end)
                   on user.Id equals order.Payer
               select new
               {
                   user.Id,
                   user.Name,
                   user.Area,
                   OrderId = order.Id,
                   order.Payer,
                   order.CreationTime
               };
            return Ok(await sql1.ToListAsync());
        }
        public async Task<IActionResult> QueryJoin2()
        {
            var begin = new DateTime(2021, 3, 2);
            var end = new DateTime(2021, 4, 3);
            var sql1 = from user in _myDbContext.Set<SysUser>().Where(o => (o.Id == "1" || o.Id == "6")&&o.Area=="A")
                join order in _myDbContext.Set<Order>().Where(o => o.CreationTime >= begin && o.CreationTime <= end)
                    on user.Id equals order.Payer
                select new
                {
                    user.Id,
                    user.Name,
                    user.Area,
                    OrderId = order.Id,
                    order.Payer,
                    order.CreationTime
                };
            return Ok(await sql1.ToListAsync());
        }
        public async Task<IActionResult> Update()
        {
            var sysUser = await _myDbContext.Set<SysUser>().Where(o => o.Id == "1").FirstOrDefaultAsync();
            sysUser.Name = "new name";
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
    }
}
