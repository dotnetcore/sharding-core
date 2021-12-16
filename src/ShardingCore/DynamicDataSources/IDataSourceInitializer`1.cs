using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DynamicDataSources
{
    public interface IDataSourceInitializer<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        [Obsolete("plz use method InitConfigure(string dataSourceName, string connectionString,bool createDatabase)")]
        void InitConfigure(string dataSourceName, string connectionString);
        void InitConfigure(string dataSourceName, string connectionString,bool createDatabase);
    }
}
