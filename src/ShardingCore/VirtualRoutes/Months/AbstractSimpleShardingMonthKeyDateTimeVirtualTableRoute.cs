using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Jobs.Impls.Attributes;
using ShardingCore.TableCreator;
using ShardingCore.VirtualRoutes.Abstractions;

namespace ShardingCore.VirtualRoutes.Months
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 27 January 2021 12:27:09
* @Email: 326308290@qq.com
*/
    public abstract class AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<T> : AbstractShardingTimeKeyDateTimeVirtualTableRoute<T>,IJob where T : class
    {
        public abstract DateTime GetBeginTime();
        public override List<string> GetAllTails()
        {
            var beginTime = ShardingCoreHelper.GetCurrentMonthFirstDay(GetBeginTime());
         
            var tails=new List<string>();
            //提前创建表
            var nowTimeStamp =ShardingCoreHelper.GetCurrentMonthFirstDay(DateTime.Now);
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("begin time error");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var tail = ShardingKeyToTail(currentTimeStamp);
                tails.Add(tail);
                currentTimeStamp = ShardingCoreHelper.GetNextMonthFirstDay(currentTimeStamp);
            }
            return tails;
        }
        protected override string TimeFormatToTail(DateTime time)
        {
            return $"{time:yyyyMM}";
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var currentMonth = ShardingCoreHelper.GetCurrentMonthFirstDay(shardingKey);
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (currentMonth == shardingKey)
                        return tail => String.Compare(tail, t, StringComparison.Ordinal) < 0;
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
#if DEBUG
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");
#endif
                    return tail => true;
                }
            }
        }
        /// <summary>
        /// 每个月20号5点会创建对应的表
        /// </summary>
        [JobRun(Name = "定时创建表", Cron = "0 0 5 20 * ?", RunOnceOnStart = true)]
        public virtual void AutoShardingTableCreate()
        {
            var entityMetadataManager = (IEntityMetadataManager)ShardingContainer.GetService(typeof(IEntityMetadataManager<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualDataSource = (IVirtualDataSource)ShardingContainer.GetService(typeof(IVirtualDataSource<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualTableManager = (IVirtualTableManager)ShardingContainer.GetService(typeof(IVirtualTableManager<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var tableCreator = (IShardingTableCreator)ShardingContainer.GetService(typeof(IShardingTableCreator<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualTable = virtualTableManager.GetVirtualTable(typeof(T));
            if (virtualTable == null)
            {
                return;
            }
            var now = DateTime.Now.Date.AddDays(3);
            var tail = virtualTable.GetVirtualRoute().ShardingKeyToTail(now);
            ISet<string> dataSources = new HashSet<string>()
            {
                virtualDataSource.DefaultDataSourceName
            };
            if (entityMetadataManager.IsShardingDataSource(typeof(T)))
            {
                var virtualDataSourceRoute = virtualDataSource.GetRoute(typeof(T));
                foreach (var dataSourceName in virtualDataSourceRoute.GetAllDataSourceNames())
                {
                    dataSources.Add(dataSourceName);
                }

            }
            foreach (var dataSource in dataSources)
            {
                try
                {
                    tableCreator.CreateTable(dataSource, typeof(T), tail);
                }
                catch (Exception e)
                {
                    //ignore
                }
            }
        }
        /// <summary>
        /// 每月最后一天需要程序自己判断是不是最后一天
        /// </summary>
        [JobRun(Name = "定时添加虚拟表", Cron = "0 55 23 28,29,30,31 * ?")]
        public virtual void AutoShardingTableAdd()
        {
            var nowDateTime = DateTime.Now.Date;
            var lastMonthDay = ShardingCoreHelper.GetNextMonthFirstDay(nowDateTime).AddDays(-1);
            if (lastMonthDay.Date != nowDateTime)
                return;

            var virtualTableManager = (IVirtualTableManager)ShardingContainer.GetService(typeof(IVirtualTableManager<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualTable = virtualTableManager.GetVirtualTable(typeof(T));
            if (virtualTable == null)
            {
                return;
            }
            var now = DateTime.Now.Date.AddDays(7);
            var tail = virtualTable.GetVirtualRoute().ShardingKeyToTail(now);
            virtualTableManager.AddPhysicTable(virtualTable, new DefaultPhysicTable(virtualTable, tail));
        }

        public virtual string JobName =>
            $"{EntityMetadata?.ShardingDbContextType?.Name}:{EntityMetadata?.EntityType?.Name}";

        public virtual bool StartJob()
        {
            return false;
        }

    }
}