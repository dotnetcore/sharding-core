using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations
{
    public class ShardingGlobalConfigOptions
    {
        /// <summary>
        /// 配置id
        /// </summary>
        public string ConfigId { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        public int MaxQueryConnectionsLimit { get; set; } = Environment.ProcessorCount;
        public ConnectionModeEnum ConnectionMode { get; set; } = ConnectionModeEnum.SYSTEM_AUTO;
        /// <summary>
        /// 读写分离配置
        /// </summary>
        public ShardingReadWriteSeparationOptions ShardingReadWriteSeparationOptions { get; private set; }
        /// <summary>
        /// 默认数据源
        /// </summary>
        public string DefaultDataSourceName { get; private set; }
        /// <summary>
        /// 默认数据源链接字符串
        /// </summary>
        public string DefaultConnectionString { get; private set; }
        /// <summary>
        /// 添加默认数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddDefaultDataSource(string dataSourceName, string connectionString)
        {
            DefaultDataSourceName= dataSourceName?? throw new ArgumentNullException(nameof(dataSourceName));
            DefaultConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        public Func<IServiceProvider, IDictionary<string, string>> DataSourcesConfigure { get; private set; }
        /// <summary>
        /// 添加额外数据源
        /// </summary>
        /// <param name="extraDataSourceConfigure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddExtraDataSource(Func<IServiceProvider, IDictionary<string, string>> extraDataSourceConfigure)
        {
            DataSourcesConfigure= extraDataSourceConfigure ?? throw new ArgumentNullException(nameof(extraDataSourceConfigure));
        }
        /// <summary>
        /// 添加读写分离配置
        /// </summary>
        /// <param name="readWriteSeparationConfigure"></param>
        /// <param name="readStrategyEnum"></param>
        /// <param name="defaultEnable"></param>
        /// <param name="defaultPriority"></param>
        /// <param name="readConnStringGetStrategy"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddReadWriteSeparation(
            Func<IServiceProvider, IDictionary<string, IEnumerable<string>>> readWriteSeparationConfigure,
            ReadStrategyEnum readStrategyEnum,
            bool defaultEnable = false,
            int defaultPriority = 10,
            ReadConnStringGetStrategyEnum readConnStringGetStrategy = ReadConnStringGetStrategyEnum.LatestFirstTime)
        {
            ShardingReadWriteSeparationOptions = new ShardingReadWriteSeparationOptions();
            ShardingReadWriteSeparationOptions.ReadWriteSeparationConfigure= readWriteSeparationConfigure ?? throw new ArgumentNullException(nameof(readWriteSeparationConfigure));
            ShardingReadWriteSeparationOptions.ReadStrategy = readStrategyEnum;
            ShardingReadWriteSeparationOptions.DefaultEnable=defaultEnable;
            ShardingReadWriteSeparationOptions.DefaultPriority= defaultPriority;
            ShardingReadWriteSeparationOptions.ReadConnStringGetStrategy= readConnStringGetStrategy;
        }


        public Action<DbConnection, DbContextOptionsBuilder> ConnectionConfigure { get; private set; }
        public Action<string, DbContextOptionsBuilder> ConnectionStringConfigure { get; private set; }


        public void UseShardingQuery(Action<string, DbContextOptionsBuilder> queryConfigure)
        {
            ConnectionStringConfigure = queryConfigure ?? throw new ArgumentNullException(nameof(queryConfigure));
        }
        public void UseShardingTransaction(Action<DbConnection, DbContextOptionsBuilder> transactionConfigure)
        {
            ConnectionConfigure = transactionConfigure ?? throw new ArgumentNullException(nameof(transactionConfigure));
        }

        public Func<IServiceProvider, IShardingComparer> ReplaceShardingComparerFactory { get; private set; } = sp => new CSharpLanguageShardingComparer();
        /// <summary>
        /// 替换默认的比较器
        /// </summary>
        /// <param name="newShardingComparerFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ReplaceShardingComparer(Func<IServiceProvider, IShardingComparer> newShardingComparerFactory)
        {
            ReplaceShardingComparerFactory = newShardingComparerFactory ?? throw new ArgumentNullException(nameof(newShardingComparerFactory));
        }


    }
}
