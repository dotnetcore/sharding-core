using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 16:57:56
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ActualConnectionStringManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly bool _useReadWriteSeparation;
        private readonly IShardingReadWriteManager _shardingReadWriteManager;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        public int ReadWriteSeparationPriority { get; set; }
        public bool ReadWriteSeparation { get; set; }
        public ReadStrategyEnum ReadStrategy { get; set; }
        public ReadConnStringGetStrategyEnum ReadConnStringGetStrategy { get; set; }
        private string _cacheConnectionString;
        public ActualConnectionStringManager(IVirtualDataSource<TShardingDbContext> virtualDataSource)
        {
            _virtualDataSource=virtualDataSource;
            _shardingReadWriteManager = ShardingContainer.GetService<IShardingReadWriteManager>();
            _useReadWriteSeparation = virtualDataSource.ConnectionStringManager is ReadWriteConnectionStringManager;
            if (_useReadWriteSeparation)
            {
                ReadWriteSeparationPriority = virtualDataSource.ConfigurationParams.ReadWriteDefaultPriority.GetValueOrDefault();
                ReadWriteSeparation = virtualDataSource.ConfigurationParams.ReadWriteDefaultEnable.GetValueOrDefault();
                ReadStrategy = virtualDataSource.ConfigurationParams.ReadStrategy.GetValueOrDefault();
                ReadConnStringGetStrategy = virtualDataSource.ConfigurationParams.ReadConnStringGetStrategy.GetValueOrDefault();
            }
        }
        //public bool IsUseReadWriteSeparation()
        //{
        //    return _useReadWriteSeparation;
        //}
        public string GetConnectionString(string dataSourceName, bool isWrite)
        {
            if (isWrite)
                return GetWriteConnectionString(dataSourceName);
            if (!_useReadWriteSeparation)
            {
                return _virtualDataSource.ConnectionStringManager.GetConnectionString(dataSourceName);
            }
            else
            {
                return GetReadWriteSeparationConnectString(dataSourceName);
            }
        }
        private string GetWriteConnectionString(string dataSourceName)
        {
            return _virtualDataSource.GetConnectionString(dataSourceName);
        }

        private string GetReadWriteSeparationConnectString(string dataSourceName)
        {
            var support = ReadWriteSeparation;
            var shardingReadWriteContext = _shardingReadWriteManager.GetCurrent<TShardingDbContext>();
            if (shardingReadWriteContext != null)
            {
                support = (ReadWriteSeparationPriority >= shardingReadWriteContext.DefaultPriority)
                    ? ReadWriteSeparation
                    : shardingReadWriteContext.DefaultReadEnable;
            }

            if (support)
            {
                return GetReadWriteSeparationConnectString0(dataSourceName);
            }
            return GetWriteConnectionString(dataSourceName);
        }
        private string GetReadWriteSeparationConnectString0(string dataSourceName)
        {
            if (ReadConnStringGetStrategy == ReadConnStringGetStrategyEnum.LatestFirstTime)
            {
                if (_cacheConnectionString == null)
                    _cacheConnectionString = _virtualDataSource.ConnectionStringManager.GetConnectionString(dataSourceName);
                return _cacheConnectionString;
            }
            else if (ReadConnStringGetStrategy == ReadConnStringGetStrategyEnum.LatestEveryTime)
            {
                return _virtualDataSource.ConnectionStringManager.GetConnectionString(dataSourceName);
            }
            else
            {
                throw new ShardingCoreInvalidOperationException($"ReadWriteConnectionStringManager ReadConnStringGetStrategy:{ReadConnStringGetStrategy}");
            }

        }
    }
}
