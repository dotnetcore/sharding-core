using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 19 December 2020 19:55:24
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 抽象类型抽象出对应的条件表达式
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class AbstractShardingOperatorVirtualDataSourceRoute<TEntity, TKey> : AbstractShardingFilterVirtualDataSourceRoute<TEntity, TKey> where TEntity : class
    {
        protected override List<string> DoRouteWithPredicate(List<string> allDataSourceNames, IQueryable queryable)
        {
            //获取路由后缀表达式
            var routeParseExpression = ShardingUtil.GetRouteParseExpression(queryable, EntityMetadata, GetRouteFilter, false);
            //表达式缓存编译
            // var filter = CachingCompile(routeParseExpression);
            var filter = routeParseExpression.GetRoutePredicate();
            //通过编译结果进行过滤
            var dataSources = allDataSourceNames.Where(o => filter(o)).ToList();
            return dataSources;
        }


        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值, 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKey">分表的值</param>
        /// <param name="shardingOperator">操作</param>
        /// <param name="shardingPropertyName">操作</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        public virtual Func<string, bool> GetRouteFilter(object shardingKey,
            ShardingOperatorEnum shardingOperator, string shardingPropertyName)
        {
            if (EntityMetadata.IsMainShardingDataSourceKey(shardingPropertyName))
            {
                return GetRouteToFilter((TKey)shardingKey, shardingOperator);
            }
            else
            {
                return GetExtraRouteFilter(shardingKey, shardingOperator, shardingPropertyName);
            }
        }

        public abstract Func<string, bool> GetRouteToFilter(TKey shardingKey,
            ShardingOperatorEnum shardingOperator);

        public virtual Func<string, bool> GetExtraRouteFilter(object shardingKey,
            ShardingOperatorEnum shardingOperator, string shardingPropertyName)
        {
            throw new NotImplementedException(shardingPropertyName);
        }

        public override string RouteWithValue(object shardingKey)
        {
            var allDataSourceNames = GetAllDataSourceNames();
            var shardingKeyToDataSource = ShardingKeyToDataSourceName(shardingKey);

            var dataSources = allDataSourceNames.Where(o => o== shardingKeyToDataSource).ToList();
            if (dataSources.IsEmpty())
            {
                throw new ShardingCoreException($"sharding key route not match {EntityMetadata.EntityType} -> [{EntityMetadata.ShardingTableProperty.Name}] ->【{shardingKey}】 all data sources ->[{string.Join(",", allDataSourceNames.Select(o=>o))}]");
            }

            if (dataSources.Count > 1)
                throw new ShardingCoreException($"more than one route match data source:{string.Join(",", dataSources.Select(o => $"[{o}]"))}");
            return dataSources[0];
        }

    }
}