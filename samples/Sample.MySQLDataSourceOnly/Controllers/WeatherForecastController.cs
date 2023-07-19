using Microsoft.AspNetCore.Mvc;
using Sample.MySQLDataSourceOnly.Domain;

namespace Sample.MySQLDataSourceOnly.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly MyDbContext _myDbContext;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,MyDbContext myDbContext)
    {
        _logger = logger;
        _myDbContext = myDbContext;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var user = _myDbContext.Set<SysUser>().FirstOrDefault(o=>o.Id=="1");
        user.Name = "456"+DateTime.Now.ToString();
        _myDbContext.SaveChanges();
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}