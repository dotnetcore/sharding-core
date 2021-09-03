using System;
using System.Linq;
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
            return streamMergeContext.RouteResults.Count() > 1;
        }
        public static bool IsSingleShardingTableQuery<TEntity>(this StreamMergeContext<TEntity> streamMergeContext)
        {
            return streamMergeContext.RouteResults.First().ReplaceTables.Count(o => o.EntityType.IsShardingTable()) == 1;
        }
    }
}