using Microsoft.EntityFrameworkCore;
using Sample.AutoCreateIfPresent;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.TableExists;

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
    .AddEntityConfig(o =>
    {
        o.ThrowIfQueryRouteNotMatch = false;
        o.CreateShardingTableOnStart = true;
        o.EnsureCreatedWithOutShardingTable = true;
        o.AddShardingTableRoute<OrderByHourRoute>();
        o.AddShardingTableRoute<AreaDeviceRoute>();
    })
    .AddConfig(o =>
    {
        o.ConfigId = "c1";
        o.AddDefaultDataSource("ds0", "server=127.0.0.1;port=3306;database=shardingTest;userid=root;password=root;");
        o.UseShardingQuery((conn, b) =>
        {
            b.UseMySql(conn, new MySqlServerVersion(new Version())).UseLoggerFactory(efLogger);
        });
        o.UseShardingTransaction((conn, b) =>
        {
            b.UseMySql(conn, new MySqlServerVersion(new Version())).UseLoggerFactory(efLogger);
        });
        o.ReplaceTableEnsureManager(sp=>new MySqlTableEnsureManager<DefaultDbContext>());
    }).EnsureConfig();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}
app.Services.GetRequiredService<IShardingBootstrapper>().Start();



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