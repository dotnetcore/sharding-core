using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Sharding.EntityQueryConfigurations;
using ShardingCore.Sharding.PaginationConfigurations;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 13:59:36
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 
    /// </summary>
    public interface IVirtualTableRoute
    {
        EntityMetadata EntityMetadata { get; }
        string ShardingKeyToTail(object shardingKey);

        /// <summary>
        /// 根据查询条件路由返回物理表
        /// </summary>
        /// <param name="dataSourceRouteResult"></param>
        /// <param name="queryable"></param>
        /// <param name="isQuery"></param>
        /// <returns></returns>
        List<TableRouteUnit> RouteWithPredicate(DataSourceRouteResult dataSourceRouteResult,IQueryable queryable,bool isQuery);

        /// <summary>
        /// 根据值进行路由
        /// </summary>
        /// <param name="dataSourceRouteResult"></param>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        TableRouteUnit RouteWithValue(DataSourceRouteResult dataSourceRouteResult, object shardingKey);
        /// <summary>
        /// 获取所有的目前数据库存在的尾巴,每次路由都会调用
        /// 请不要在此处添加过于复杂的操作
        /// get all tails in the db
        /// </summary>
        /// <returns></returns>
        List<string> GetTails();
           
         /// <summary>
         /// 分页配置
         /// </summary>
          PaginationMetadata PaginationMetadata { get; }

         /// <summary>
         /// 是否启用智能分页
         /// </summary>
          bool EnablePagination {get; }// PaginationMetadata != null;
         /// <summary>
         /// 查询配置
         /// </summary>
          EntityQueryMetadata EntityQueryMetadata { get; }
         /// <summary>
         /// 是否启用表达式分片配置
         /// </summary>
          bool EnableEntityQuery { get; }

    }

    public interface IVirtualTableRoute<TEntity> : IVirtualTableRoute, IEntityMetadataTableConfiguration<TEntity> where TEntity : class
    {
        /// <summary>
        /// 返回null就是表示不开启分页配置
        /// 譬如如下配置
        /// <code><![CDATA[builder.PaginationSequence(o => o.DateOfMonth)
        /// .UseRouteComparer(Comparer<string>.Default)
        /// .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch)
        /// .UseAppendIfOrderNone(10);]]></code>
        /// 表示当前分页配置如果使用<code>DateOfMonth</code>属性排序并且只要符合属性属于这个对象或者名称一样并且只要是第一个排序即可
        /// 当前排序将可以使用高性能排序，这边需要设置一个当前属性<code>DateOfMonth</code> order by asc的时候当前分表多表是如何排序的
        /// </summary>
        /// <returns></returns>
        IPaginationConfiguration<TEntity> CreatePaginationConfiguration();
        /// <summary>
        /// 配置查询
        /// <code><![CDATA[
        /// //DateOfMonth的排序和月份分片的后缀一致所以用true如果false,无果无关就不需要配置
        /// builder.ShardingTailComparer(Comparer<string>.Default, false);
        /// //表示他是倒叙
        /// builder.AddOrder(o => o.DateOfMonth, false);
        /// //
        /// builder.AddDefaultSequenceQueryTrip(false, CircuitBreakerMethodNameEnum.FirstOrDefault, CircuitBreakerMethodNameEnum.Enumerator);
        ///
        /// builder.AddConnectionsLimit(2, LimitMethodNameEnum.First, LimitMethodNameEnum.FirstOrDefault, LimitMethodNameEnum.Any, LimitMethodNameEnum.LastOrDefault, LimitMethodNameEnum.Last, LimitMethodNameEnum.Max, LimitMethodNameEnum.Min);
        /// builder.AddConnectionsLimit(1, LimitMethodNameEnum.Enumerator);]]></code>
        /// </summary>
        /// <returns></returns>
        IEntityQueryConfiguration<TEntity> CreateEntityQueryConfiguration();

    }
}