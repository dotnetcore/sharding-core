using Microsoft.EntityFrameworkCore;
using Sample.SqlServer.DbContexts;
using ShardingCore;
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;

namespace Sample.SqlServer
{
    public class DbContextHelper
    {
        public static DbContext CreateDbContextByString(string connectionString,IShardingRuntimeContext shardingRuntimeContext)
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<DefaultShardingDbContext>();
            dbContextOptionsBuilder.UseSqlServer(connectionString).UseSharding<DefaultShardingDbContext>(shardingRuntimeContext);
            return new DefaultShardingDbContext(dbContextOptionsBuilder.Options);
        }
    }
}
