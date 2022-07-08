using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using ShardingCore.Core.ServiceProviders;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    public class SimpleVirtualDataSourceConfigurationParams: AbstractVirtualDataSourceConfigurationParams
    {
        private readonly ShardingConfigOptions _options;
        public override int MaxQueryConnectionsLimit { get; }
        public override ConnectionModeEnum ConnectionMode { get; }
        public override string DefaultDataSourceName { get; }
        public override string DefaultConnectionString { get; }
        public override IDictionary<string, string> ExtraDataSources { get; }
        public override IDictionary<string, ReadNode[]> ReadWriteNodeSeparationConfigs { get; }
        public override ReadStrategyEnum? ReadStrategy { get; }
        public override bool? ReadWriteDefaultEnable { get; }
        public override int? ReadWriteDefaultPriority { get; }
        public override ReadConnStringGetStrategyEnum? ReadConnStringGetStrategy { get; }

        public SimpleVirtualDataSourceConfigurationParams(IShardingProvider shardingProvider,ShardingConfigOptions options)
        {
            _options = options;
            MaxQueryConnectionsLimit = options.MaxQueryConnectionsLimit;
            ConnectionMode = options.ConnectionMode;
            DefaultDataSourceName = options.DefaultDataSourceName;
            DefaultConnectionString = options.DefaultConnectionString;
            ExtraDataSources = options.DataSourcesConfigure?.Invoke(shardingProvider)??new ConcurrentDictionary<string, string>();
          

            if (options.ShardingReadWriteSeparationOptions != null)
            {
                if (options.ShardingReadWriteSeparationOptions.ReadWriteNodeSeparationConfigure != null)
                {
                    var readConfig = options.ShardingReadWriteSeparationOptions.ReadWriteNodeSeparationConfigure?.Invoke(shardingProvider);
                    if (readConfig != null)
                    {
                        ReadWriteNodeSeparationConfigs = readConfig.ToDictionary(kv=>kv.Key,kv=>kv.Value.ToArray());
                    }
                }
                else
                {
                    var nodeConfig = options.ShardingReadWriteSeparationOptions.ReadWriteSeparationConfigure?.Invoke(shardingProvider);
                    if (nodeConfig != null)
                    {
                        ReadWriteNodeSeparationConfigs = nodeConfig.ToDictionary(kv => kv.Key,
                            kv => kv.Value.Select(o => new ReadNode(Guid.NewGuid().ToString("n"), o)).ToArray());
                    }
                }
                ReadStrategy = options.ShardingReadWriteSeparationOptions.ReadStrategy;
                ReadWriteDefaultEnable = options.ShardingReadWriteSeparationOptions.DefaultEnable;
                ReadWriteDefaultPriority = options.ShardingReadWriteSeparationOptions.DefaultPriority;
                ReadConnStringGetStrategy = options.ShardingReadWriteSeparationOptions.ReadConnStringGetStrategy;
            }
        }

        public override DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString,
            DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if(_options.ConnectionStringConfigure==null)
            {
                throw new InvalidOperationException($"unknown {nameof(UseDbContextOptionsBuilder)} by connection string");
            }
            _options.ConnectionStringConfigure.Invoke(connectionString, dbContextOptionsBuilder);
            return dbContextOptionsBuilder;
        }

        public override DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection,
            DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if (_options.ConnectionConfigure == null )
            {
                throw new InvalidOperationException($"unknown {nameof(UseDbContextOptionsBuilder)} by connection");
            }
            _options.ConnectionConfigure.Invoke(dbConnection, dbContextOptionsBuilder);
            return dbContextOptionsBuilder;
        }

        public override void UseShellDbContextOptionBuilder(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            _options.ShellDbContextConfigure?.Invoke(dbContextOptionsBuilder);
        }

        public override void UseExecutorDbContextOptionBuilder(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            _options.ExecutorDbContextConfigure?.Invoke(dbContextOptionsBuilder);
        }
    }
}
