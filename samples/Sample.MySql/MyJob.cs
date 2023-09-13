using Microsoft.EntityFrameworkCore;
using Sample.MySql.DbContexts;
using ShardingCore;
using ShardingCore.Core.RuntimeContexts;

namespace Sample.MySql;

public class MyJob :IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IShardingRuntimeContext _shardingRuntimeContext;

    public MyJob(IServiceProvider serviceProvider,IShardingRuntimeContext shardingRuntimeContext)
    {
        _serviceProvider = serviceProvider;
        _shardingRuntimeContext = shardingRuntimeContext;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // using (var serviceScope = _serviceProvider.CreateScope())
        // {
        //     var defaultShardingDbContext = serviceScope.ServiceProvider.GetService<DefaultShardingDbContext>();
        // }
        //
        // var dbContextOptionsBuilder = new DbContextOptionsBuilder<DefaultShardingDbContext>();
        // dbContextOptionsBuilder.UseSharding(_shardingRuntimeContext);
        // using (var dbcontext = new DefaultShardingDbContext(dbContextOptionsBuilder.Options))
        // {
        //     
        // }

        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}