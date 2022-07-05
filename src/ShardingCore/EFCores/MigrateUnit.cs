using Microsoft.EntityFrameworkCore;

namespace ShardingCore.EFCores
{
    
    public class MigrateUnit
    {
        public MigrateUnit(DbContext shellDbContext, string dataSourceName)
        {
            ShellDbContext = shellDbContext;
            DataSourceName = dataSourceName;
        }

        public DbContext ShellDbContext { get; }
        public string DataSourceName { get; }
    }
}
