using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableCreator;
using ShardingCore.VirtualRoutes.Abstractions;

namespace ShardingCore.VirtualRoutes.Days
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 27 January 2021 08:56:38
* @Email: 326308290@qq.com
*/
    public abstract class AbstractSimpleShardingDayKeyLongVirtualTableRoute<T>:AbstractShardingTimeKeyLongVirtualTableRoute<T>,IJob where T:class
    {
        public abstract DateTime GetBeginTime();
        public override List<string> GetAllTails()
        {
            var beginTime = GetBeginTime().Date;
         
            var tails=new List<string>();
            //提前创建表
            var nowTimeStamp = DateTime.Now.Date;
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("begin time error");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var currentTimeStampLong=ShardingCoreHelper.ConvertDateTimeToLong(currentTimeStamp);
                var tail = TimeFormatToTail(currentTimeStampLong);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddDays(1);
            }
            return tails;
        }
        protected override string TimeFormatToTail(long time)
        {
            var dateTime=ShardingCoreHelper.ConvertLongToDateTime(time);
            return $"{dateTime:yyyyMMdd}";
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(long shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var dateTime = ShardingCoreHelper.ConvertLongToDateTime(shardingKey);
                    var shardingKeyDate = dateTime.Date;
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (shardingKeyDate == dateTime)
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
        /// 每天5点执行,启动的时候执行以下
        /// </summary>
        [JobRun(Name = "定时创建表", Cron = "0 0 5 * * ?", RunOnceOnStart = true)]
        public virtual void AutoShardingTableCreate()
        {
            var virtualDataSource = (IVirtualDataSource)ShardingContainer.GetService(typeof(IVirtualDataSource<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualTableManager = (IVirtualTableManager)ShardingContainer.GetService(typeof(IVirtualTableManager<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var tableCreator = (IShardingTableCreator)ShardingContainer.GetService(typeof(IShardingTableCreator<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualTable = virtualTableManager.GetVirtualTable(typeof(T));
            if (virtualTable == null)
            {
                return;
            }
            var now = DateTime.Now.Date.AddDays(1);
            var tail = virtualTable.GetVirtualRoute().ShardingKeyToTail(now);
            try
            {
                tableCreator.CreateTable(virtualDataSource.DefaultDataSourceName, typeof(T), tail);
            }
            catch (Exception e)
            {
                //ignore
            }
        }
        /// <summary>
        /// 每天晚上23点59分执行
        /// </summary>
        [JobRun(Name = "定时添加虚拟表", Cron = "0 55 23 * * ?")]
        public virtual void AutoShardingTableAdd()
        {
            var virtualTableManager = (IVirtualTableManager)ShardingContainer.GetService(typeof(IVirtualTableManager<>).GetGenericType0(EntityMetadata.ShardingDbContextType));
            var virtualTable = virtualTableManager.GetVirtualTable(typeof(T));
            if (virtualTable == null)
            {
                return;
            }
            var now = DateTime.Now.Date.AddDays(1);
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