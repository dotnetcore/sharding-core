using Microsoft.EntityFrameworkCore;
using Sample.NoShardingMultiLevel;
using ShardingCore;
using ShardingCore.Bootstrapers;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.TableExists;

ILoggerFactory efLogger = LoggerFactory.Create(builder =>
{
    builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
});
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddDbContext<DefaultDbContext>(builder1 =>
//    builder1.UseSqlServer("Data Source=localhost;Initial Catalog=dbmulti;Integrated Security=True;")
//        .UseLoggerFactory(efLogger));
builder.Services.AddShardingDbContext<DefaultDbContext>((conStr, builder) => builder.UseSqlServer(conStr).UseLoggerFactory(efLogger))
    .Begin(o =>
    {
        o.CreateShardingTableOnStart = true;
        o.EnsureCreatedWithOutShardingTable = true;
    })
    .AddShardingTransaction((connection, builder) =>
    {
        builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
    })
    .AddDefaultDataSource("ds0", "Data Source=localhost;Initial Catalog=dbmulti;Integrated Security=True;")
    .AddTableEnsureManager(sp => new SqlServerTableEnsureManager<DefaultDbContext>())
    .AddReadWriteSeparation(sp =>
    {
        return new Dictionary<string, IEnumerable<string>>()
        {
            {
                "ds0", new List<string>()
                {
                    "Data Source=localhost;Initial Catalog=dbmulti;Integrated Security=True;"
                }
            }
        };
    }, ReadStrategyEnum.Loop, defaultEnable: true)
    .End();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.Services.GetRequiredService<IShardingBootstrapper>().Start();
app.UseAuthorization();

app.MapControllers();

app.Run();
