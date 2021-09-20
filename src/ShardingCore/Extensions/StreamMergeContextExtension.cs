using System;
using System.Linq;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Sharding;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 02 September 2021 20:46:24
* @Email: 326308290@qq.com
*/
    public static class StreamMergeContextExtension
    {
        /// <summary>
        /// 本次查询是否涉及到分表
        /// </summary>
        /// <param name="streamMergeContext"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static bool IsShardingQuery<TEntity>(this StreamMergeContext<TEntity> streamMergeContext)
        {
            return streamMergeContext.TableRouteResults.Count() > 1;
        }
        public static bool IsSingleShardingTableQuery<TEntity>(this StreamMergeContext<TEntity> streamMergeContext)
        {
            return streamMergeContext.TableRouteResults.First().ReplaceTables.Count(o => o.EntityType.IsShardingTable()) == 1;
        }

        public static IVirtualTableManager GetVirtualTableManager<TEntity>(this StreamMergeContext<TEntity> streamMergeContext)
        {
            return (IVirtualTableManager)ShardingContainer.GetService(
                typeof(IVirtualTableManager<>).GetGenericType0(streamMergeContext.GetShardingDbContext().GetType()));
        }
    }
}