using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Logger;
using ShardingCore.TableCreator;

namespace ShardingCore.VirtualRoutes.Abstractions
{
    [ExcludeFromCodeCoverage]
    public abstract class AbstractShardingAutoCreateOperatorVirtualTableRoute<TEntity, TKey> : AbstractShardingOperatorVirtualTableRoute<TEntity, TKey>, IJob where TEntity : class
    {
        private static readonly ILogger<AbstractShardingAutoCreateOperatorVirtualTableRoute<TEntity, TKey>> _logger =
            InternalLoggerFactory.CreateLogger<AbstractShardingAutoCreateOperatorVirtualTableRoute<TEntity, TKey>>();

        /// <summary>
        /// 不可以设置一样
        /// </summary>
        public virtual string JobName =>
            $"{EntityMetadata?.ShardingDbContextType?.Name}:{EntityMetadata?.EntityType?.Name}";

        /// <summary>
        /// 是否需要自动创建按时间分表的路由
        /// </summary>
        /// <returns></returns>
        public abstract bool AutoCreateTableByTime();
        /// <summary>
        /// 显示错误日志
        /// </summary>
        public virtual bool DoLogError => false;
        /// <summary>
        /// 默认会在设置时间后10分钟获取tail
        /// </summary>
        public virtual int IncrementMinutes => 10;
        /// <summary>
        /// 重写改方法后请一起重写IncrementMinutes值，比如你按月分表但是你设置cron表达式为月中的时候建表，
        /// 那么会在月中的时候 <code>DateTime.Now.AddMinutes(IncrementMinutes);</code>来获取tail会导致还是当月的所以不会建表
        /// </summary>
        /// <returns></returns>
        public abstract string[] GetCronExpressions();

        /// <summary>
        /// 当前时间转成
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        protected abstract string ConvertNowToTail(DateTime now);
        public virtual Task ExecuteAsync()
        {
            var virtualTableManager = (IVirtualTableManager)ShardingContainer.GetService(typeof(IVirtualTableManager<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualTable = virtualTableManager.GetVirtualTable(typeof(TEntity));
            _logger.LogDebug($"get {typeof(TEntity).Name}'s virtualTable ");
            if (virtualTable == null)
            {
                _logger.LogDebug($" {typeof(TEntity).Name}'s virtualTable  is null");
                return Task.CompletedTask;
            }
            var entityMetadataManager = (IEntityMetadataManager)ShardingContainer.GetService(typeof(IEntityMetadataManager<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var tableCreator = (IShardingTableCreator)ShardingContainer.GetService(typeof(IShardingTableCreator<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualDataSourceManager = (IVirtualDataSourceManager)ShardingContainer.GetService(typeof(IVirtualDataSourceManager<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var allVirtualDataSources = virtualDataSourceManager.GetAllVirtualDataSources();
            var now = DateTime.Now.AddMinutes(IncrementMinutes);
            var tail = ConvertNowToTail(now);
//必须先执行AddPhysicTable在进行CreateTable
            virtualTableManager.AddPhysicTable(virtualTable, new DefaultPhysicTable(virtualTable, tail));
            foreach (var virtualDataSource in allVirtualDataSources)
            {
                ISet<string> dataSources = new HashSet<string>();
                if (entityMetadataManager.IsShardingDataSource(typeof(TEntity)))
                {
                    var virtualDataSourceRoute = virtualDataSource.GetRoute(typeof(TEntity));
                    foreach (var dataSourceName in virtualDataSourceRoute.GetAllDataSourceNames())
                    {
                        dataSources.Add(dataSourceName);
                    }
                }
                else
                {
                    dataSources.Add(virtualDataSource.DefaultDataSourceName);
                }
                _logger.LogInformation($"auto create table data source names:[{string.Join(",", dataSources)}]");
                using (virtualDataSourceManager.CreateScope(virtualDataSource.ConfigId))
                {
                    foreach (var dataSource in dataSources)
                    {
                        try
                        {
                            _logger.LogInformation($"begin table tail:[{tail}],entity:[{typeof(TEntity).Name}]");
                            tableCreator.CreateTable(dataSource, typeof(TEntity), tail);
                            _logger.LogInformation($"succeed table tail:[{tail}],entity:[{typeof(TEntity).Name}]");
                        }
                        catch (Exception e)
                        {
                            //ignore
                            _logger.LogInformation($"warning table tail:[{tail}],entity:[{typeof(TEntity).Name}]");
                            if (DoLogError)
                                _logger.LogError(e, $"{dataSource} {typeof(TEntity).Name}'s create table error ");
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

    }
}
