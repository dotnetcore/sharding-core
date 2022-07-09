using Microsoft.EntityFrameworkCore;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.ServiceProviders;

namespace Sample.ShardingConsole;

public class MyDbContextCreator:ActivatorDbContextCreator<MyDbContext>
{
    public override DbContext GetShellDbContext(IShardingProvider shardingProvider)
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        dbContextOptionsBuilder.UseDefaultSharding<MyDbContext>(ShardingProvider.ShardingRuntimeContext);
        return new MyDbContext(dbContextOptionsBuilder.Options);
    }
}