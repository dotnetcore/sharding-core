using Microsoft.EntityFrameworkCore;
using Sample.SqlServer.DbContexts;
using ShardingCore;

namespace Sample.SqlServer
{
    public class DbContextHelper
    {
        public static DbContext CreateDbContextByString(string connectionString)
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<DefaultShardingDbContext>();
            dbContextOptionsBuilder.UseSqlServer(connectionString).UseSharding<DefaultShardingDbContext>();
            return new DefaultShardingDbContext(dbContextOptionsBuilder.Options);
        }
    }
}
