using Microsoft.EntityFrameworkCore;
using Sample.NoShardingMultiLevel;
using ShardingCore;
using ShardingCore.Bootstrappers;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;

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

builder.Services.AddShardingDbContext<DefaultDbContext>()
    .AddEntityConfig(o =>
    {
    })
    .AddConfig(op =>
    {
        op.UseShardingQuery((conStr, builder) =>
        {
            builder.UseSqlServer(conStr).UseLoggerFactory(efLogger);
        });
        op.UseShardingTransaction((connection, builder) =>
        {
            builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
        });
        op.AddDefaultDataSource("ds0", "Data Source=localhost;Initial Catalog=dbmulti;Integrated Security=True;");
        op.AddReadWriteSeparation(sp =>
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
        }, ReadStrategyEnum.Loop, defaultEnable: true);
    }).ReplaceService<ITableEnsureManager,SqlServerTableEnsureManager>().EnsureConfig();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.Services.UseAutoShardingCreate();
app.Services.UseAutoTryCompensateTable();
app.UseAuthorization();

app.MapControllers();

app.Run();
