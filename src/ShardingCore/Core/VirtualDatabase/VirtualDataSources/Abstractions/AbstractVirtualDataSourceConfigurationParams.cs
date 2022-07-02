using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions
{
    public abstract class AbstractVirtualDataSourceConfigurationParams:IVirtualDataSourceConfigurationParams
    {
        public virtual int MaxQueryConnectionsLimit { get; } = Environment.ProcessorCount;
        public virtual ConnectionModeEnum ConnectionMode { get; } = ConnectionModeEnum.SYSTEM_AUTO;
        public abstract string DefaultDataSourceName { get; }
        public abstract string DefaultConnectionString { get; }
        public virtual IDictionary<string, string> ExtraDataSources { get; }=new ConcurrentDictionary<string, string>();
        public virtual IDictionary<string, ReadNode[]> ReadWriteNodeSeparationConfigs { get; }
        public virtual ReadStrategyEnum? ReadStrategy { get; }
        public virtual bool? ReadWriteDefaultEnable { get; }
        public virtual int? ReadWriteDefaultPriority { get; }
        public virtual ReadConnStringGetStrategyEnum? ReadConnStringGetStrategy { get; }

        public abstract DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString,
            DbContextOptionsBuilder dbContextOptionsBuilder);
        
        public abstract DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection,
            DbContextOptionsBuilder dbContextOptionsBuilder);

        public abstract void UseShellDbContextOptionBuilder(DbContextOptionsBuilder dbContextOptionsBuilder);

        public abstract void UseExecutorDbContextOptionBuilder(DbContextOptionsBuilder dbContextOptionsBuilder);

        public virtual bool UseReadWriteSeparation()
        {
            return ReadWriteNodeSeparationConfigs!=null;
        }
        
    }
}
