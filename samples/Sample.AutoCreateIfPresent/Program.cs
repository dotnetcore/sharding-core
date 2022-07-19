using Microsoft.EntityFrameworkCore;
using Sample.AutoCreateIfPresent;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;


var join = string.Join(Environment.NewLine,Enumerable.Range(14,100).Select(o=>o+";"));
ILoggerFactory efLogger = LoggerFactory.Create(builder =>
{
    builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
});

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddShardingDbContext<DefaultDbContext>()
    .UseRouteConfig(o =>
    {
        o.AddShardingTableRoute<OrderByHourRoute>();
        o.AddShardingTableRoute<AreaDeviceRoute>();
    })
    .UseConfig(o =>
    {
        o.ThrowIfQueryRouteNotMatch = false;
        o.AddDefaultDataSource("ds0", "server=127.0.0.1;port=3306;database=shardingTest;userid=root;password=root;");
        o.UseShardingQuery((conn, b) =>
        {
            b.UseMySql(conn, new MySqlServerVersion(new Version())).UseLoggerFactory(efLogger);
        });
        o.UseShardingTransaction((conn, b) =>
        {
            b.UseMySql(conn, new MySqlServerVersion(new Version())).UseLoggerFactory(efLogger);
        });
    })
    .ReplaceService<ITableEnsureManager,MySqlTableEnsureManager>().AddShardingCore();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}
app.Services.UseAutoShardingCreate();
app.Services.UseAutoTryCompensateTable();



app.UseAuthorization();

app.MapControllers();
using (var serviceScope = app.Services.CreateScope())
{
    var defaultDbContext = serviceScope.ServiceProvider.GetService<DefaultDbContext>();
    
    var orderByHour = new OrderByHour();
    orderByHour.Id = Guid.NewGuid().ToString("n");
    orderByHour.Name=$"Name:"+ Guid.NewGuid().ToString("n");
    var dateTime = DateTime.Now;
    orderByHour.CreateTime = dateTime.AddHours(new Random().Next(1, 20));
    await defaultDbContext.AddAsync(orderByHour);
    await defaultDbContext.SaveChangesAsync();
}

app.Run();