using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

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
        ///// <summary>
        ///// 单表查询
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <param name="streamMergeContext"></param>
        ///// <returns></returns>
        //public static bool IsSingleShardingTableQuery<TEntity>(this StreamMergeContext<TEntity> streamMergeContext)
        //{
        //    return streamMergeContext.TableRouteResults.First().ReplaceTables.Count(o => o.EntityType.IsShardingTable()) == 1;
        //}
        ///// <summary>
        ///// 本次查询仅包含一个对象的分表分库
        ///// </summary>
        ///// <typeparam name="TEntity"></typeparam>
        ///// <param name="streamMergeContext"></param>
        ///// <returns></returns>
        //public static bool IsSingleShardingQuery<TEntity>(this StreamMergeContext<TEntity> streamMergeContext)
        //{
        //    return streamMergeContext.GetOriginalQueryable().ParseQueryableRoute().Count(o=>o.IsShardingTable()||o.IsShardingDataSource())==1;
        //}
        public static bool IsSupportPaginationQuery<TShardingDbContext,TEntity>(this StreamMergeContext<TEntity> streamMergeContext) where TShardingDbContext:DbContext,IShardingDbContext
        {
            var entityMetadataManager = ShardingContainer.GetService<IEntityMetadataManager<TShardingDbContext>>();

            var queryEntities = streamMergeContext.GetOriginalQueryable().ParseQueryableEntities();
            //仅一个对象支持分库或者分表的组合
            return queryEntities.Count(o=>(entityMetadataManager.IsShardingDataSource(o) &&!entityMetadataManager.IsShardingTable(o)) ||(entityMetadataManager.IsShardingDataSource(o)&& entityMetadataManager.IsShardingTable(o))|| (!entityMetadataManager.IsShardingDataSource(o) && entityMetadataManager.IsShardingTable(o))) ==1;
        }
    }
}