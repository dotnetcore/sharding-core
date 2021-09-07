using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 10:37:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReadWriteConnectionStringManager<TShardingDbContext> : IConnectionStringManager where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IShardingReadWriteManager _shardingReadWriteManager;
        public Type ShardingDbContextType => typeof(TShardingDbContext);
        private IShardingConnectionStringResolver _shardingConnectionStringResolver;
        private string _cacheConnectionString;


        public ReadWriteConnectionStringManager(IShardingReadWriteManager shardingReadWriteManager, IEnumerable<IShardingConnectionStringResolver> shardingConnectionStringResolvers)
        {
            _shardingReadWriteManager = shardingReadWriteManager;
            _shardingConnectionStringResolver = shardingConnectionStringResolvers.FirstOrDefault(o => o.ShardingDbContextType == ShardingDbContextType) ?? throw new ArgumentNullException($"{ShardingDbContextType.FullName}:{nameof(shardingConnectionStringResolvers)}");
        }
        public string GetConnectionString(IShardingDbContext shardingDbContext)
        {
            if (!(shardingDbContext is IShardingReadWriteSupport shardingReadWriteSupport))
            {
                return shardingDbContext.GetConnectionString();
            }
            var shardingReadWriteContext = _shardingReadWriteManager.GetCurrent(ShardingDbContextType);
            var support = shardingReadWriteSupport.ReadWriteSupport;
            if (shardingReadWriteContext != null)
            {
                support = (shardingReadWriteSupport.ReadWritePriority >= shardingReadWriteContext.DefaultPriority)
                    ? shardingReadWriteSupport.ReadWriteSupport
                    : shardingReadWriteContext.DefaultReadEnable;
            }

            if (support)
            {
                return GetReadConnectionString0(shardingReadWriteSupport);
            }
            return shardingReadWriteSupport.GetWriteConnectionString();
        }
        private string GetReadConnectionString0(IShardingReadWriteSupport shardingReadWriteSupport)
        {
            var readConnStringGetStrategy = shardingReadWriteSupport.GetReadConnStringGetStrategy();
            if (readConnStringGetStrategy == ReadConnStringGetStrategyEnum.LatestFirstTime)
            {
                if (_cacheConnectionString == null)
                    _cacheConnectionString = _shardingConnectionStringResolver.GetConnectionString();
                return _cacheConnectionString;
            }
            else if (readConnStringGetStrategy == ReadConnStringGetStrategyEnum.LatestEveryTime)
            {
                return _shardingConnectionStringResolver.GetConnectionString();
            }
            else
            {
                throw new InvalidOperationException($"ReadWriteConnectionStringManager:{readConnStringGetStrategy}");
            }
        }
    }
}
