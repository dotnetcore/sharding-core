using Microsoft.EntityFrameworkCore;
using Samples.Oracle.Infrastructure;
using Samples.Oracle.Infrastructure.VirtualRoutes.TableRoutes;
using ShardingCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddShardingDbContext<DemoDbContext>().UseRouteConfig(provider =>
{
    provider.AddShardingTableRoute<StudentCreationTimeVirtualTableRoute>();
}).UseConfig(options =>
{
    options.ThrowIfQueryRouteNotMatch = false;
    options.UseShardingQuery((connectionString, builder) =>
    {
        builder.UseOracle(connectionString);
    });
    options.UseShardingTransaction((connection, builder) =>
    {
        builder.UseOracle(connection);
    });
    options.AddDefaultDataSource("Default", builder.Configuration.GetConnectionString("Default"));
}).AddShardingCore();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    DemoDbContext dbContext = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await dbContext.Database.MigrateAsync();
}
app.Services.UseAutoTryCompensateTable();

app.MapControllers();

app.Run();
