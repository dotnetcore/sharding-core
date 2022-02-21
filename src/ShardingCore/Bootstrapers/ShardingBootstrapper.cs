using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Extensions;
using ShardingCore.Jobs;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;

namespace ShardingCore.Bootstrapers
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
        private readonly IEnumerable<IDbContextTypeCollector> _dbContextTypeCollectors;
        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();

        public ShardingBootstrapper(IServiceProvider serviceProvider,ILogger<ShardingBootstrapper> logger,IEnumerable<IDbContextTypeCollector> dbContextTypeCollectors)
        {
            _logger = logger;
            _dbContextTypeCollectors = dbContextTypeCollectors;
            ShardingContainer.SetServices(serviceProvider);
        }
        /// <summary>
        /// Æô¶¯
        /// </summary>
        public void Start()
        {
            if (!_doOnlyOnce.IsUnDo())
                return;
            _logger.LogDebug("sharding core starting......");
            foreach (var dbContextTypeCollector in _dbContextTypeCollectors)
            {
                var instance = (IShardingDbContextBootstrapper)ShardingContainer.CreateInstance(typeof(ShardingDbContextBootstrapper<>).GetGenericType0(dbContextTypeCollector.ShardingDbContextType));
                _logger.LogDebug($"{dbContextTypeCollector.ShardingDbContextType}  start init......");
                instance.Init();
                _logger.LogDebug($"{dbContextTypeCollector.ShardingDbContextType}  complete init");
            }

            var jobManager = ShardingContainer.GetService<IJobManager>();
            if (jobManager != null && jobManager.HasAnyJob())
            {
                Task.Factory.StartNew(async () =>
                {
                    await ShardingContainer.GetService<JobRunnerService>().StartAsync();
                }, TaskCreationOptions.LongRunning);
            }
        }

    }
}