using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Extensions.ShardingQueryableExtensions;

namespace Sample.AutoCreateIfPresent.Controllers;

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

    public WeatherForecastController(ILogger<WeatherForecastController> logger,DefaultDbContext defaultDbContext)
    {
        _logger = logger;
        _defaultDbContext = defaultDbContext;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
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
        var s = Guid.NewGuid().ToString();
        var anyAsync = await _defaultDbContext.Set<AreaDevice>().AnyAsync(o=>o.Area==s);
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