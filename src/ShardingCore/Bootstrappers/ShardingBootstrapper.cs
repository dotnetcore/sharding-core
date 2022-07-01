using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Extensions;
using ShardingCore.Jobs;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Logger;
using ShardingCore.Sharding.MergeEngines.ParallelControl;

namespace ShardingCore.Bootstrappers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 21 December 2020 09:10:07
    * @Email: 326308290@qq.com
    */
    public class ShardingBootstrapper : IShardingBootstrapper
    {
        private readonly ILogger<ShardingBootstrapper> _logger;
        private readonly IShardingProvider _shardingProvider;
        private readonly IDbContextTypeCollector _dbContextTypeCollector;
        private readonly IJobManager _jobManager;
        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();

        public ShardingBootstrapper(IShardingProvider shardingProvider,IDbContextTypeCollector dbContextTypeCollector,IJobManager jobManager)
        {
            _logger = InternalLoggerFactory.DefaultFactory.CreateLogger<ShardingBootstrapper>();
            _shardingProvider = shardingProvider;
            _dbContextTypeCollector = dbContextTypeCollector;
            _jobManager = jobManager;
        }
        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (!_doOnlyOnce.IsUnDo())
                return;
            _logger.LogDebug("sharding core starting......");

            var instanceType = typeof(ShardingDbContextBootstrapper<>).GetGenericType0(_dbContextTypeCollector.ShardingDbContextType);
            var instance = (IShardingDbContextBootstrapper)_shardingProvider.CreateInstance(instanceType);
            _logger.LogDebug($"{_dbContextTypeCollector.ShardingDbContextType}  start init......");
            instance.Initialize();
            _logger.LogDebug($"{_dbContextTypeCollector.ShardingDbContextType}  complete init");

            if (_jobManager != null && _jobManager.HasAnyJob())
            {
                Task.Factory.StartNew(async () =>
                {
                    await _shardingProvider.GetRequiredService<JobRunnerService>().StartAsync();
                }, TaskCreationOptions.LongRunning);
            }
        }

    }
}