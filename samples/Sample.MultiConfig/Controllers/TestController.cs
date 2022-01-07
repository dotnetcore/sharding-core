using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.MultiConfig.Domain.Entities;

namespace Sample.MultiConfig.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController:ControllerBase
    {
        private readonly MultiConfigDbContext _multiConfigDbContext;

        public TestController(MultiConfigDbContext multiConfigDbContext)
        {
            _multiConfigDbContext = multiConfigDbContext;
        }
        public async Task<IActionResult> Add()
        {
            var order = new Order();
            order.Id = Guid.NewGuid().ToString("n");
            order.Name = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            order.CreateTime=DateTime.Now;
            await _multiConfigDbContext.AddAsync(order);
            await _multiConfigDbContext.SaveChangesAsync();
            return Ok();
        }
        public async Task<IActionResult> Query()
        {
            var listAsync = await _multiConfigDbContext.Set<Order>().ToListAsync();
            return Ok(listAsync);
        }
    }
}
