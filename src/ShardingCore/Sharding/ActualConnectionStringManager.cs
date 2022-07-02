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
    public class ActualConnectionStringManager
    {
        private readonly bool _useReadWriteSeparation;
        private readonly IShardingReadWriteManager _shardingReadWriteManager;
        private readonly IVirtualDataSource _virtualDataSource;
        public int ReadWriteSeparationPriority { get; set; }
        public bool ReadWriteSeparation { get; set; }
        public ReadStrategyEnum ReadStrategy { get; set; }
        public ReadConnStringGetStrategyEnum ReadConnStringGetStrategy { get; set; }
        private string _cacheConnectionString;
        public ActualConnectionStringManager(IShardingReadWriteManager shardingReadWriteManager,IVirtualDataSource virtualDataSource)
        {
            _shardingReadWriteManager = shardingReadWriteManager;
            _virtualDataSource=virtualDataSource;
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
            string readNodeName = null;
            var hasConfig = false;
            var shardingReadWriteContext = _shardingReadWriteManager.GetCurrent();
            if (shardingReadWriteContext != null)
            {
                var dbFirst = ReadWriteSeparationPriority >= shardingReadWriteContext.DefaultPriority;
                support = dbFirst
                    ? ReadWriteSeparation
                    : shardingReadWriteContext.DefaultReadEnable;
                if (!dbFirst&& support)
                {
                    hasConfig = shardingReadWriteContext.TryGetDataSourceReadNode(dataSourceName, out readNodeName);
                }
            }

            if (support)
            {
                return GetReadWriteSeparationConnectString0(dataSourceName, hasConfig?readNodeName:null);
            }
            return GetWriteConnectionString(dataSourceName);
        }
        private string GetReadWriteSeparationConnectString0(string dataSourceName,string readNodeName)
        {
            if (_virtualDataSource.ConnectionStringManager is IReadWriteConnectionStringManager
                readWriteConnectionStringManager)
            {
                if (ReadConnStringGetStrategy == ReadConnStringGetStrategyEnum.LatestFirstTime)
                {
                    if (_cacheConnectionString == null)
                        _cacheConnectionString = readWriteConnectionStringManager.GetReadNodeConnectionString(dataSourceName, readNodeName);
                    return _cacheConnectionString;
                }
                else if (ReadConnStringGetStrategy == ReadConnStringGetStrategyEnum.LatestEveryTime)
                {
                    return readWriteConnectionStringManager.GetReadNodeConnectionString(dataSourceName,readNodeName);
                }
                else
                {
                    throw new ShardingCoreInvalidOperationException($"ReadWriteConnectionStringManager ReadConnStringGetStrategy:{ReadConnStringGetStrategy}");
                }
            }
            else
            {
                throw new ShardingCoreInvalidOperationException($"virtual data source connection string manager is not [{nameof(IReadWriteConnectionStringManager)}]");
            }

        }
    }
}
