using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    public class SimpleVirtualDataSourceConfigurationParams<TShardingDbContext>: AbstractVirtualDataSourceConfigurationParams<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ShardingGlobalConfigOptions _options;
        public override string ConfigId { get; }
        public override int Priority { get; }
        public override int MaxQueryConnectionsLimit { get; }
        public override ConnectionModeEnum ConnectionMode { get; }
        public override string DefaultDataSourceName { get; }
        public override string DefaultConnectionString { get; }
        public override IDictionary<string, string> ExtraDataSources { get; }
        public override IDictionary<string, IEnumerable<string>> ReadWriteSeparationConfigs { get; }
        public override ReadStrategyEnum? ReadStrategy { get; }
        public override bool? ReadWriteDefaultEnable { get; }
        public override int? ReadWriteDefaultPriority { get; }
        public override ReadConnStringGetStrategyEnum? ReadConnStringGetStrategy { get; }
        public override IShardingComparer ShardingComparer { get; }

        public SimpleVirtualDataSourceConfigurationParams(IServiceProvider serviceProvider,ShardingGlobalConfigOptions options)
        {
            _options = options;
            ConfigId = options.ConfigId;
            Priority = options.Priority;
            MaxQueryConnectionsLimit = options.MaxQueryConnectionsLimit;
            ConnectionMode = options.ConnectionMode;
            DefaultDataSourceName = options.DefaultDataSourceName;
            DefaultConnectionString = options.DefaultConnectionString;
            ExtraDataSources = options.DataSourcesConfigure?.Invoke(serviceProvider)??new ConcurrentDictionary<string, string>();
            ShardingComparer = options.ReplaceShardingComparerFactory?.Invoke(serviceProvider) ??
                               new CSharpLanguageShardingComparer();
            if (options.ShardingReadWriteSeparationOptions != null)
            {
                ReadWriteSeparationConfigs = options.ShardingReadWriteSeparationOptions.ReadWriteSeparationConfigure?.Invoke(serviceProvider);
                ReadStrategy = options.ShardingReadWriteSeparationOptions.ReadStrategy;
                ReadWriteDefaultEnable = options.ShardingReadWriteSeparationOptions.DefaultEnable;
                ReadWriteDefaultPriority = options.ShardingReadWriteSeparationOptions.DefaultPriority;
                ReadConnStringGetStrategy = options.ShardingReadWriteSeparationOptions.ReadConnStringGetStrategy;
            }
        }

        public override DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString,
            DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            _options.ConnectionStringConfigure.Invoke(connectionString, dbContextOptionsBuilder);
            return dbContextOptionsBuilder;
        }

        public override DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection,
            DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            _options.ConnectionConfigure.Invoke(dbConnection, dbContextOptionsBuilder);
            return dbContextOptionsBuilder;
        }
    }
}
