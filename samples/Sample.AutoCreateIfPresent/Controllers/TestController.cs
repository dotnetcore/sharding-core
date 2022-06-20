using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Sample.AutoCreateIfPresent.Controllers;

public class AA
{
    public string Id { get; set; }
    public DateTime Time { get; set; }
}
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
        var aas = new List<AA>();
        var ids = new []{"雪花id1","雪花id2"};
        var time1 = new DateTime(2021,1,1);
        var time2 = new DateTime(2022,1,1);
        var enumerable = aas.Where(o=>ids.Contains(o.Id)&&o.Time>=time1&&o.Time<=time2);
        var enumerable1 = aas.Where(o=>o.Id=="雪花id1"||o.Id=="雪花id2");
        var enumerable2 = aas.Where(o=>o.Id=="雪花id1"&&o.Id=="雪花id2");

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