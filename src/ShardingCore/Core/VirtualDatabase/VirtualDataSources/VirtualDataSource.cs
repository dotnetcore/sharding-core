using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingEnumerableQueries;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Utils;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 05 February 2021 15:21:04
    * @Email: 326308290@qq.com
    */
    public class VirtualDataSource : IVirtualDataSource
    {
        public IVirtualDataSourceConfigurationParams ConfigurationParams { get; }
        public IConnectionStringManager ConnectionStringManager { get; }


        private readonly IPhysicDataSourcePool _physicDataSourcePool;
        public string DefaultDataSourceName { get; private set; }
        public string DefaultConnectionString { get; private set; }
        public bool UseReadWriteSeparation { get; }

        public VirtualDataSource( IVirtualDataSourceConfigurationParams configurationParams,IReadWriteConnectorFactory readWriteConnectorFactory)
        {
            Check.NotNull(configurationParams, nameof(configurationParams));
            Check.NotNull(configurationParams.ExtraDataSources, nameof(configurationParams.ExtraDataSources));
            if(configurationParams.MaxQueryConnectionsLimit<=0)
                throw new ArgumentOutOfRangeException(nameof(configurationParams.MaxQueryConnectionsLimit));
            ConfigurationParams = configurationParams;
            _physicDataSourcePool = new PhysicDataSourcePool();
            //添加数据源
            AddPhysicDataSource(new DefaultPhysicDataSource(ConfigurationParams.DefaultDataSourceName, ConfigurationParams.DefaultConnectionString, true));
            foreach (var extraDataSource in ConfigurationParams.ExtraDataSources)
            {
                AddPhysicDataSource(new DefaultPhysicDataSource(extraDataSource.Key, extraDataSource.Value, false));
            }
            UseReadWriteSeparation = ConfigurationParams.UseReadWriteSeparation();
            if (UseReadWriteSeparation)
            {
                CheckReadWriteSeparation();
                ConnectionStringManager = new ReadWriteConnectionStringManager(this,readWriteConnectorFactory);
            }
            else
            {
                ConnectionStringManager = new DefaultConnectionStringManager(this);

            }
        }

        private void CheckReadWriteSeparation()
        {
            if (!ConfigurationParams.ReadStrategy.HasValue)
            {
                throw new ArgumentException(nameof(ConfigurationParams.ReadStrategy));
            }
            if (!ConfigurationParams.ReadConnStringGetStrategy.HasValue)
            {
                throw new ArgumentException(nameof(ConfigurationParams.ReadConnStringGetStrategy));
            }
            if (!ConfigurationParams.ReadWriteDefaultEnable.HasValue)
            {
                throw new ArgumentException(nameof(ConfigurationParams.ReadWriteDefaultEnable));
            }
            if (!ConfigurationParams.ReadWriteDefaultPriority.HasValue)
            {
                throw new ArgumentException(nameof(ConfigurationParams.ReadWriteDefaultPriority));
            }
        }

        /// <summary>
        /// 获取默认数据源
        /// </summary>
        /// <returns></returns>
        public IPhysicDataSource GetDefaultDataSource()
        {
            return GetPhysicDataSource(DefaultDataSourceName);
        }
        /// <summary>
        /// 获取物理数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreNotFoundException"></exception>
        public IPhysicDataSource GetPhysicDataSource(string dataSourceName)
        {
            Check.NotNull(dataSourceName, $"data source name is null,plz confirm {dataSourceName} add in virtual data source");
            var dataSource = _physicDataSourcePool.TryGet(dataSourceName);
            if (null == dataSource)
                throw new ShardingCoreNotFoundException($"data source:[{dataSourceName}]");

            return dataSource;
        }
        /// <summary>
        /// 获取所有的数据源名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllDataSourceNames()
        {
            return _physicDataSourcePool.GetAllDataSourceNames();
        }

        /// <summary>
        /// 根据数据源名称获取链接字符串
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreNotFoundException"></exception>
        public string GetConnectionString(string dataSourceName)
        {
            if (IsDefault(dataSourceName))
                return DefaultConnectionString;
            return GetPhysicDataSource(dataSourceName).ConnectionString;
        }

        /// <summary>
        /// 添加物理数据源
        /// </summary>
        /// <param name="physicDataSource"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">�ظ�����Ĭ������Դ</exception>
        public bool AddPhysicDataSource(IPhysicDataSource physicDataSource)
        {
            if (physicDataSource.IsDefault)
            {
                if (!string.IsNullOrWhiteSpace(DefaultDataSourceName))
                {
                    throw new ShardingCoreInvalidOperationException($"default data source name:[{DefaultDataSourceName}],add physic default data source name:[{physicDataSource.DataSourceName}]");
                }
                DefaultDataSourceName = physicDataSource.DataSourceName;
                DefaultConnectionString = physicDataSource.ConnectionString;
            }

            return _physicDataSourcePool.TryAdd(physicDataSource);
        }
        /// <summary>
        /// 判断数据源名称是否是默认的数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public bool IsDefault(string dataSourceName)
        {
            return DefaultDataSourceName == dataSourceName;
        }
        /// <summary>
        /// 检查虚拟数据源是否包含默认值
        /// </summary>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        public void CheckVirtualDataSource()
        {
            if (string.IsNullOrWhiteSpace(DefaultDataSourceName))
                throw new ShardingCoreInvalidOperationException(
                    $"virtual data source not inited {nameof(DefaultDataSourceName)} in IShardingDbContext null");
            if (string.IsNullOrWhiteSpace(DefaultConnectionString))
                throw new ShardingCoreInvalidOperationException(
                    $"virtual data source not inited {nameof(DefaultConnectionString)} in IShardingDbContext null");
        }

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString,
            DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            var doUseDbContextOptionsBuilder = ConfigurationParams.UseDbContextOptionsBuilder(connectionString, dbContextOptionsBuilder);
            doUseDbContextOptionsBuilder.UseInnerDbContextSharding();
            ConfigurationParams.UseExecutorDbContextOptionBuilder(dbContextOptionsBuilder);
            return doUseDbContextOptionsBuilder;
        }

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection,
            DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            var doUseDbContextOptionsBuilder = ConfigurationParams.UseDbContextOptionsBuilder(dbConnection, dbContextOptionsBuilder);
            doUseDbContextOptionsBuilder.UseInnerDbContextSharding();
            ConfigurationParams.UseExecutorDbContextOptionBuilder(dbContextOptionsBuilder);
            return doUseDbContextOptionsBuilder;
        }

        public IDictionary<string, string> GetDataSources()
        {
            return _physicDataSourcePool.GetDataSources();
        }
    }
}