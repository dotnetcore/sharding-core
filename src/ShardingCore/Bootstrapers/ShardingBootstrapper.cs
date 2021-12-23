using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IEnumerable<IDbContextTypeCollector> _dbContextTypeCollectors;
        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();

        public ShardingBootstrapper(IServiceProvider serviceProvider,IEnumerable<IDbContextTypeCollector> dbContextTypeCollectors)
        {
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
            foreach (var dbContextTypeCollector in _dbContextTypeCollectors)
            {
                var instance = (IShardingDbContextBootstrapper)ShardingContainer.CreateInstance(typeof(ShardingDbContextBootstrapper<>).GetGenericType0(dbContextTypeCollector.ShardingDbContextType));
                instance.Init();
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