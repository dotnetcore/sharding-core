using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.DynamicDataSources;
using ShardingCore.Exceptions;

namespace ShardingCore.Extensions
{

    public static class ShardingRuntimeExtension
    {
        public static void UseAutoShardingCreate(this IShardingRuntimeContext shardingRuntimeContext)
        {
            shardingRuntimeContext.CheckRequirement();
            shardingRuntimeContext.AutoShardingCreate();
        }
        /// <summary>
        /// 自动尝试补偿表
        /// </summary>
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="parallelCount"></param>
        public static void UseAutoTryCompensateTable(this IShardingRuntimeContext shardingRuntimeContext, int? parallelCount = null)
        {
            shardingRuntimeContext.CheckRequirement();
            var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            var dataSourceInitializer = shardingRuntimeContext.GetDataSourceInitializer();
            var shardingConfigOptions = shardingRuntimeContext.GetShardingConfigOptions();
            var compensateTableParallelCount = parallelCount ?? shardingConfigOptions.CompensateTableParallelCount;
            if (compensateTableParallelCount <= 0)
            {
                throw new ShardingCoreInvalidOperationException($"compensate table parallel count must > 0");
            }
            var allDataSourceNames = virtualDataSource.GetAllDataSourceNames();
            var partitionMigrationUnits = allDataSourceNames.Partition(compensateTableParallelCount);
            foreach (var migrationUnits in partitionMigrationUnits)
            {
                var migrateUnits = migrationUnits.Select(o => new InitConfigureUnit(o)).ToList();
                ExecuteInitConfigureUnit(dataSourceInitializer, migrateUnits);
            }
        }

        private static void ExecuteInitConfigureUnit(IDataSourceInitializer dataSourceInitializer,
            List<InitConfigureUnit> initConfigureUnits)
        {
            var initConfigureTasks = initConfigureUnits.Select(o =>
            {
                return Task.Run(() => { dataSourceInitializer.InitConfigure(o.DataSourceName, true, true); });
            }).ToArray();
            Task.WaitAll(initConfigureTasks);
        }
    
    } 
}