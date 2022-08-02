using Microsoft.EntityFrameworkCore;

namespace ShardingCore.EFCores
{
    /// <summary>
    /// 迁移执行单元
    /// </summary>
    public class MigrateUnit
    {
        public MigrateUnit(DbContext shellDbContext, string dataSourceName)
        {
            ShellDbContext = shellDbContext;
            DataSourceName = dataSourceName;
        }
        /// <summary>
        /// 壳dbcontext
        /// </summary>
        public DbContext ShellDbContext { get; }
        /// <summary>
        /// 数据源名称
        /// </summary>
        public string DataSourceName { get; }
    }
}
