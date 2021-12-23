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
        private readonly IConnectionStringManager<TShardingDbContext> _connectionStringManager;
        private readonly IReadWriteOptions<TShardingDbContext> _readWriteOptions;
        private readonly bool _useReadWriteSeparation;
        private readonly IShardingReadWriteManager _shardingReadWriteManager;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        public int ReadWriteSeparationPriority { get; set; }
        public bool ReadWriteSeparation { get; set; }
        private string _cacheConnectionString;
        public ActualConnectionStringManager()
        {
            _virtualDataSource=ShardingContainer.GetService<IVirtualDataSource<TShardingDbContext>>();
            _connectionStringManager = ShardingContainer.GetService<IConnectionStringManager<TShardingDbContext>>();
            _shardingReadWriteManager = ShardingContainer.GetService<IShardingReadWriteManager>();
            _useReadWriteSeparation = _connectionStringManager is ReadWriteConnectionStringManager<TShardingDbContext>;
            if (_useReadWriteSeparation)
            {
                _readWriteOptions = ShardingContainer.GetService<IReadWriteOptions<TShardingDbContext>>();
                if (_readWriteOptions != null)
                {
                    ReadWriteSeparationPriority = _readWriteOptions.ReadWritePriority;
                    ReadWriteSeparation = _readWriteOptions.ReadWriteSupport;
                }
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
                return _connectionStringManager.GetConnectionString(dataSourceName);
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
            if (_readWriteOptions.ReadConnStringGetStrategy == ReadConnStringGetStrategyEnum.LatestFirstTime)
            {
                if (_cacheConnectionString == null)
                    _cacheConnectionString = _connectionStringManager.GetConnectionString(dataSourceName);
                return _cacheConnectionString;
            }
            else if (_readWriteOptions.ReadConnStringGetStrategy == ReadConnStringGetStrategyEnum.LatestEveryTime)
            {
                return _connectionStringManager.GetConnectionString(dataSourceName);
            }
            else
            {
                throw new ShardingCoreInvalidOperationException($"ReadWriteConnectionStringManager:{_readWriteOptions.ReadConnStringGetStrategy}");
            }

        }
    }
}
