using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision;
using ShardingCore.Sharding.ShardingComparision.Abstractions;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingConfigurations.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    public class SimpleVirtualDataSourceConfigurationParams<TShardingDbContext>: AbstractVirtualDataSourceConfigurationParams<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ShardingConfigOptions<TShardingDbContext> _options;
        private readonly IShardingEntityConfigOptions<TShardingDbContext> _shardingEntityConfigOptions;
        public override string ConfigId { get; }
        public override int Priority { get; }
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
        public override IShardingComparer ShardingComparer { get; }
        public override ITableEnsureManager TableEnsureManager { get; }

        public SimpleVirtualDataSourceConfigurationParams(IServiceProvider serviceProvider,ShardingConfigOptions<TShardingDbContext> options)
        {
            _shardingEntityConfigOptions = serviceProvider.GetService<IShardingEntityConfigOptions<TShardingDbContext>>();
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
            TableEnsureManager = options.TableEnsureManagerFactory?.Invoke(serviceProvider) ??
                                 new EmptyTableEnsureManager<TShardingDbContext>();
            if (options.ShardingReadWriteSeparationOptions != null)
            {
                if (options.ShardingReadWriteSeparationOptions.ReadWriteNodeSeparationConfigure != null)
                {
                    var readConfig = options.ShardingReadWriteSeparationOptions.ReadWriteNodeSeparationConfigure?.Invoke(serviceProvider);
                    if (readConfig != null)
                    {
                        ReadWriteNodeSeparationConfigs = readConfig.ToDictionary(kv=>kv.Key,kv=>kv.Value.ToArray());
                    }
                }
                else
                {
                    var nodeConfig = options.ShardingReadWriteSeparationOptions.ReadWriteSeparationConfigure?.Invoke(serviceProvider);
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
            if(_options.ConnectionStringConfigure==null&&_shardingEntityConfigOptions.ConnectionStringConfigure==null)
            {
                throw new InvalidOperationException($"unknown {nameof(UseDbContextOptionsBuilder)} by connection string");
            }
            if (_options.ConnectionStringConfigure != null)
            {
                _options.ConnectionStringConfigure.Invoke(connectionString, dbContextOptionsBuilder);
            }
            else
            {
                _shardingEntityConfigOptions.ConnectionStringConfigure.Invoke(connectionString, dbContextOptionsBuilder);
            }
            return dbContextOptionsBuilder;
        }

        public override DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection,
            DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if (_options.ConnectionConfigure == null && _shardingEntityConfigOptions.ConnectionConfigure == null)
            {
                throw new InvalidOperationException($"unknown {nameof(UseDbContextOptionsBuilder)} by connection");
            }
            if (_options.ConnectionConfigure != null)
            {
                _options.ConnectionConfigure.Invoke(dbConnection, dbContextOptionsBuilder);
            }
            else
            {
                _shardingEntityConfigOptions.ConnectionConfigure.Invoke(dbConnection, dbContextOptionsBuilder);
            }
            return dbContextOptionsBuilder;
        }

        public override void UseShellDbContextOptionBuilder(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if (_options.ShellDbContextConfigure == null && _shardingEntityConfigOptions.ShellDbContextConfigure == null)
            {
                return;
            }

            if (_options.ShellDbContextConfigure != null)
            {
                _options.ShellDbContextConfigure.Invoke(dbContextOptionsBuilder);
            }
            else
            {
                _shardingEntityConfigOptions.ShellDbContextConfigure?.Invoke(dbContextOptionsBuilder);
            }
        }

        public override void UseExecutorDbContextOptionBuilder(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if (_options.ExecutorDbContextConfigure == null && _shardingEntityConfigOptions.ExecutorDbContextConfigure == null)
            {
                return;
            }

            if (_options.ExecutorDbContextConfigure != null)
            {
                _options.ExecutorDbContextConfigure.Invoke(dbContextOptionsBuilder);
            }
            else
            {
                _shardingEntityConfigOptions.ExecutorDbContextConfigure?.Invoke(dbContextOptionsBuilder);
            }
        }
    }
}
