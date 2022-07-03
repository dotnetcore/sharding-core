using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingAll.Entities;

namespace Sample.SqlServerShardingAll.Controllers
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
        public async Task<IActionResult> Query()
        {

            // #region 动态数据源
            //
            // var virtualDataSource = ShardingContainer.GetRequiredCurrentVirtualDataSource<MyDbContext>();
            //
            // var virtualDataSourceRoute1 = virtualDataSource.GetRoute(typeof(Order));
            // virtualDataSourceRoute1.AddDataSourceName("D");
            // var virtualDataSourceRoute2 = virtualDataSource.GetRoute(typeof(SysUser));
            // virtualDataSourceRoute2.AddDataSourceName("D");
            // DynamicDataSourceHelper.DynamicAppendDataSource<MyDbContext>(virtualDataSource,"D", "连接字符串");
            // #endregion

            var sysUser =await _myDbContext.Set<SysUser>().Where(o=>o.Id=="1").FirstOrDefaultAsync();
            var sysUserA1 =await _myDbContext.Set<SysUser>().Where(o=>o.Id=="1" && o.Area == "A").FirstOrDefaultAsync();
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
           var end = new DateTime(2021, 5, 9);
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
            var end = new DateTime(2021, 5, 9);
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
