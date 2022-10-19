using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Jobs;
using ShardingCore.Jobs.Abstaractions;

using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.ParallelTables;

namespace ShardingCore.Bootstrappers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 21 December 2020 09:10:07
    * @Email: 326308290@qq.com
    */
    internal class ShardingBootstrapper : IShardingBootstrapper
    {
        private readonly IShardingProvider _shardingProvider;
        private readonly DoOnlyOnce _onlyOnce=new DoOnlyOnce();
        public ShardingBootstrapper(IShardingProvider shardingProvider)
        {
            _shardingProvider = shardingProvider;
        }
        public void AutoShardingCreate()
        {
            if (!_onlyOnce.IsUnDo())
                return;
            StartAutoShardingJob();
        }

        private void StartAutoShardingJob()
        {
            var jobRunnerService = _shardingProvider.GetRequiredService<JobRunnerService>(false);
            Task.Factory.StartNew(async () =>
            {
                await jobRunnerService.StartAsync();
            }, TaskCreationOptions.LongRunning);
        }

    }
}