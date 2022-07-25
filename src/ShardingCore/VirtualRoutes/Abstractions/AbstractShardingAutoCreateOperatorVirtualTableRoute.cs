using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.Collections;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Jobs.Abstaractions;

using ShardingCore.TableCreator;

namespace ShardingCore.VirtualRoutes.Abstractions
{
    /// <summary>
    /// 分片字段追加
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class
        AbstractShardingAutoCreateOperatorVirtualTableRoute<TEntity, TKey> :
            AbstractShardingOperatorVirtualTableRoute<TEntity, TKey>, ITailAppendable, IJob where TEntity : class
    {
        private  readonly object APPEND_LOCK = new object();


        private readonly SafeReadAppendList<string> _tails = new SafeReadAppendList<string>();

        public override List<string> GetTails()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            return _tails.CopyList;
        }

        protected abstract List<string> CalcTailsOnStart();

        public bool Append(string tail)
        {
            lock (APPEND_LOCK)
            {
                if (!_tails.Contains(tail))
                {
                    _tails.Append(tail);
                    return true;
                }

                return false;
            }
        }

        public override void Initialize(EntityMetadata entityMetadata, IShardingProvider shardingProvider)
        {
            base.Initialize(entityMetadata, shardingProvider);
            var calcTailsOnStart = CalcTailsOnStart();
            foreach (var tail in calcTailsOnStart)
            {
                Append(tail);
            }
        }

        /// <summary>
        /// 不可以设置一样
        /// </summary>
        public virtual string JobName =>
            $"{GetType().Name}:{EntityMetadata?.EntityType?.Name}";

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

        public virtual string[] GetJobCronExpressions()
        {
            return GetCronExpressions();
        }
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
            var logger=RouteShardingProvider
                .GetService<ILogger<AbstractShardingAutoCreateOperatorVirtualTableRoute<TEntity, TKey>>>();
            logger.LogDebug($"get {typeof(TEntity).Name}'s route execute job ");

            var entityMetadataManager = RouteShardingProvider.GetRequiredService<IEntityMetadataManager>();
            var tableCreator = RouteShardingProvider.GetRequiredService<IShardingTableCreator>();
            var virtualDataSource = RouteShardingProvider.GetRequiredService<IVirtualDataSource>();
            var dataSourceRouteManager = RouteShardingProvider.GetRequiredService<IDataSourceRouteManager>();
            var now = DateTime.Now.AddMinutes(IncrementMinutes);
            var tail = ConvertNowToTail(now);
//必须先执行AddPhysicTable在进行CreateTable
            Append(tail);
            ISet<string> dataSources = new HashSet<string>();
            if (entityMetadataManager.IsShardingDataSource(typeof(TEntity)))
            {
                var virtualDataSourceRoute = dataSourceRouteManager.GetRoute(typeof(TEntity));
                foreach (var dataSourceName in virtualDataSourceRoute.GetAllDataSourceNames())
                {
                    dataSources.Add(dataSourceName);
                }
            }
            else
            {
                dataSources.Add(virtualDataSource.DefaultDataSourceName);
            }

            logger.LogInformation($"auto create table data source names:[{string.Join(",", dataSources)}]");

            foreach (var dataSource in dataSources)
            {
                try
                {
                    logger.LogInformation($"begin table tail:[{tail}],entity:[{typeof(TEntity).Name}]");
                    tableCreator.CreateTable(dataSource, typeof(TEntity), tail);
                    logger.LogInformation($"succeed table tail:[{tail}],entity:[{typeof(TEntity).Name}]");
                }
                catch (Exception e)
                {
                    //ignore
                    logger.LogInformation($"warning table tail:[{tail}],entity:[{typeof(TEntity).Name}]");
                    if (DoLogError)
                        logger.LogError(e, $"{dataSource} {typeof(TEntity).Name}'s create table error ");
                }
            }

            return Task.CompletedTask;
        }

        public bool AppendJob()
        {
            return AutoCreateTableByTime();
        }
    }
}