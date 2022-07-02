using Microsoft.EntityFrameworkCore;
using Sample.MultiConfig;
using ShardingCore;
using ShardingCore.Bootstrappers;

var builder = WebApplication.CreateBuilder(args);
//
// // Add services to the container.
//
//  ILoggerFactory efLogger = LoggerFactory.Create(builder =>
// {
//     builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
// });
// builder.Services.AddControllers();
// builder.Services.AddShardingDbContext<MultiConfigDbContext>()
//     .AddEntityConfig(op =>
//     {
//         op.AddShardingTableRoute<OrderIdModVirtualTableRoute>();
//     })
//     .AddConfig(op =>
//     {
//         // op.ConfigId = "c1";
//         op.UseShardingQuery((conStr, b) =>
//         {
//             b.UseSqlServer(conStr).UseLoggerFactory(efLogger);
//         });
//         op.UseShardingTransaction((conn, b) =>
//         {
//             b.UseSqlServer(conn).UseLoggerFactory(efLogger);
//         });
//         op.AddDefaultDataSource("ds0",
//             "Data Source=localhost;Initial Catalog=MultiConfigSharding;Integrated Security=True;");
//         // op.ReplaceTableEnsureManager(sp => new SqlServerTableEnsureManager<MultiConfigDbContext>());
//     })
//     .AddConfig(op =>
//     {
//         op.ConfigId = "c2";
//         op.UseShardingQuery((conStr, b) =>
//         {
//             b.UseMySql(conStr, new MySqlServerVersion(new Version())).UseLoggerFactory(efLogger);
//         });
//         op.UseShardingTransaction((conn, b) =>
//         {
//             b.UseMySql(conn, new MySqlServerVersion(new Version())).UseLoggerFactory(efLogger);
//         });
//         op.AddDefaultDataSource("ds0",
//             "server=127.0.0.1;port=3306;database=MultiConfigSharding;userid=root;password=L6yBtV6qNENrwBy7;");
//         op.ReplaceTableEnsureManager(sp => new MySqlTableEnsureManager<MultiConfigDbContext>());
//     }).EnsureMultiConfig();
//
var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// app.Services.GetRequiredService<IShardingBootstrapper>().Start();
app.UseAuthorization();
// app.UseMiddleware<TenantSelectMiddleware>();
//
app.MapControllers();
//
app.Run();
