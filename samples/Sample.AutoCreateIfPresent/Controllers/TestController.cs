using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Sample.AutoCreateIfPresent.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TestController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly DefaultDbContext _defaultDbContext;

    public TestController(ILogger<WeatherForecastController> logger,DefaultDbContext defaultDbContext)
    {
        _logger = logger;
        _defaultDbContext = defaultDbContext;
    }

    public IActionResult HelloWorld()
    {
        return Ok("hello world");
    }

    public async Task<IActionResult> Query()
    {
        var list =await _defaultDbContext.Set<OrderByHour>().ToListAsync();
        return Ok(list);
    }
    public async Task<IActionResult> Insert()
    {
        var orderByHour = new OrderByHour();
        orderByHour.Id = Guid.NewGuid().ToString("n");
        orderByHour.Name=$"Name:"+ Guid.NewGuid().ToString("n");
        var dateTime = DateTime.Now;
        orderByHour.CreateTime = dateTime.AddHours(new Random().Next(1, 20));
        await _defaultDbContext.AddAsync(orderByHour);
        await _defaultDbContext.SaveChangesAsync();
        return Ok();
    }
    public async Task<IActionResult> Query1()
    {
        var list = await _defaultDbContext.Set<AreaDevice>().ToListAsync();
        return Ok(list);
    }
    public async Task<IActionResult> Insert1()
    {
        var list = new List<string>(){"A","B","C","D","E", "F", "G" };
        var orderByHour = new AreaDevice();
        orderByHour.Id = Guid.NewGuid().ToString("n");
        orderByHour.Area = list[new Random().Next(0, list.Count)];
        var dateTime = DateTime.Now;
        orderByHour.CreateTime = dateTime.AddHours(new Random().Next(1, 20));
        await _defaultDbContext.AddAsync(orderByHour);
        await _defaultDbContext.SaveChangesAsync();
        return Ok();
    }
    
}