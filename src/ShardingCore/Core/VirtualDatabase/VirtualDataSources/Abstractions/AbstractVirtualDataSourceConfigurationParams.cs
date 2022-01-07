using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.TableExists.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions
{
    public abstract class AbstractVirtualDataSourceConfigurationParams<TShardingDbContext>:IVirtualDataSourceConfigurationParams<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public abstract string ConfigId { get; }
        public abstract int Priority { get; }
        public abstract int MaxQueryConnectionsLimit { get; }
        public abstract ConnectionModeEnum ConnectionMode { get; }
        public abstract string DefaultDataSourceName { get; }
        public abstract string DefaultConnectionString { get; }
        public abstract IDictionary<string, string> ExtraDataSources { get; }
        public abstract IDictionary<string, IEnumerable<string>> ReadWriteSeparationConfigs { get; }
        public abstract ReadStrategyEnum? ReadStrategy { get; }
        public abstract bool? ReadWriteDefaultEnable { get; }
        public abstract int? ReadWriteDefaultPriority { get; }
        public abstract ReadConnStringGetStrategyEnum? ReadConnStringGetStrategy { get; }
        public abstract IShardingComparer ShardingComparer { get; }
        public abstract ITableEnsureManager TableEnsureManager { get; }


        public abstract DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString,
            DbContextOptionsBuilder dbContextOptionsBuilder);
        
        public abstract DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection,
            DbContextOptionsBuilder dbContextOptionsBuilder);

        public virtual bool UseReadWriteSeparation()
        {
            return ReadWriteSeparationConfigs!=null;
        }
        
    }
}
