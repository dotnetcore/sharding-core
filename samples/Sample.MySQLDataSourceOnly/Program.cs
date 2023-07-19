using Microsoft.EntityFrameworkCore;
using Sample.MySQLDataSourceOnly.Domain;
using ShardingCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddShardingDbContext<MyDbContext>()
    .UseRouteConfig(o =>
    {
        //o.CreateShardingTableOnStart = true;
        //o.EnsureCreatedWithOutShardingTable = true;
        o.AddShardingDataSourceRoute<SysUserVirtualDataSourceRoute>();
    })
    .UseConfig((sp,op) =>
    {
        var loggerFactory = sp.ApplicationServiceProvider.GetService<ILoggerFactory>();
        //op.ConfigId = "c1";
        op.UseShardingQuery((conStr, builder) =>
        {
            builder.UseMySql(conStr,new MySqlServerVersion(new Version()))
                .UseLoggerFactory(loggerFactory).EnableSensitiveDataLogging();
        });
        op.UseShardingTransaction((connection, builder) =>
        {
            builder.UseMySql(connection,new MySqlServerVersion(new Version()))
                .UseLoggerFactory(loggerFactory).EnableSensitiveDataLogging();;
        });
        //op.ReplaceTableEnsureManager(sp => new SqlServerTableEnsureManager<MyDbContext>());
        op.AddDefaultDataSource("A", @"server=127.0.0.1;port=3306;database=onlyds1;userid=root;password=root;");
        op.AddExtraDataSource(sp =>
        {
            return new Dictionary<string, string>()
            {
                {
                    "B",
                    @"server=127.0.0.1;port=3306;database=onlyds2;userid=root;password=root;"
                },
                {
                    "C",
                    @"server=127.0.0.1;port=3306;database=onlyds3;userid=root;password=root;"
                },
            };
        });
    }).AddShardingCore();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Services.UseAutoTryCompensateTable();
// using (var serviceScope = app.Services.CreateScope())
// {
//     var myDbContext = serviceScope.ServiceProvider.GetService<MyDbContext>();
//     myDbContext.Database.EnsureCreated();
//     myDbContext.Add(new SysUser() { Id = "1", Area = "A", Name = "name" });
//     myDbContext.SaveChanges();
// }
app.Run();