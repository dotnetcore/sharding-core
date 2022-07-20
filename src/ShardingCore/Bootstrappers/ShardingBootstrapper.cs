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
    public class ShardingBootstrapper : IShardingBootstrapper
    {
        private readonly IShardingProvider _shardingProvider;
        private readonly IDbContextCreator _dbContextCreator;
        private readonly DoOnlyOnce _onlyOnce=new DoOnlyOnce();
        public ShardingBootstrapper(IShardingProvider shardingProvider,IDbContextCreator dbContextCreator)
        {
            _shardingProvider = shardingProvider;
            _dbContextCreator = dbContextCreator;
        }
        public void AutoShardingCreate()
        {
            if (!_onlyOnce.IsUnDo())
                return;
            CheckRequirement();
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
        private void CheckRequirement()
        {
            try
            {
                using (var scope = _shardingProvider.CreateScope())
                {
                    using (var dbContext = _dbContextCreator.GetShellDbContext(scope.ServiceProvider))
                    {
                        if (dbContext == null)
                        {
                            throw new ShardingCoreInvalidOperationException(
                                $"cant get shell db context,plz override {nameof(IDbContextCreator)}.{nameof(IDbContextCreator.GetShellDbContext)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ShardingCoreInvalidOperationException(
                    $"cant get shell db context,plz override {nameof(IDbContextCreator)}.{nameof(IDbContextCreator.GetShellDbContext)}",
                    ex);
            }
        }

    }
}