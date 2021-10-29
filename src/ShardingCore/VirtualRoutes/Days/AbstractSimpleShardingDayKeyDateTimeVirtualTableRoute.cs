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
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Jobs.Impls.Attributes;
using ShardingCore.TableCreator;
using ShardingCore.VirtualRoutes.Abstractions;

namespace ShardingCore.VirtualRoutes.Days
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 27 January 2021 08:41:05
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractSimpleShardingDayKeyDateTimeVirtualTableRoute<T>:AbstractShardingTimeKeyDateTimeVirtualTableRoute<T> where T:class
    {
        /// <summary>
        /// begin time use fixed time eg.new DateTime(20xx,xx,xx)
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetBeginTime();
        /// <summary>
        /// return all tails in database
        /// </summary>
        /// <returns></returns>
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
                var tail = ShardingKeyToTail(currentTimeStamp);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddDays(1);
            }
            return tails;
        }
        protected override string TimeFormatToTail(DateTime time)
        {
            return $"{time:yyyyMMdd}";
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {

            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var shardingKeyDate = shardingKey.Date;
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (shardingKeyDate == shardingKey)
                        return tail =>String.Compare(tail, t, StringComparison.Ordinal) < 0;
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) <= 0;
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