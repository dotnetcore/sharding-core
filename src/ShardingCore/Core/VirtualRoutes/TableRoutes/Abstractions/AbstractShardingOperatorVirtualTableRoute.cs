using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 19 December 2020 19:55:24
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractShardingOperatorVirtualTableRoute<TEntity, TKey> : AbstractShardingFilterVirtualTableRoute<TEntity, TKey> where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceRouteResult"></param>
        /// <param name="allPhysicTables"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected override List<TableRouteUnit> DoRouteWithPredicate(DataSourceRouteResult dataSourceRouteResult, IQueryable queryable)
        {
            //获取路由后缀表达式
            var routeParseExpression = ShardingUtil.GetRouteParseExpression(queryable, EntityMetadata, GetRouteFilter,true);
            //表达式缓存编译
            // var filter =CachingCompile(routeParseExpression);
            var filter =routeParseExpression.GetRoutePredicate();
            var sqlRouteUnits = dataSourceRouteResult.IntersectDataSources.SelectMany(dataSourceName=>
                GetTails()
                    .Where(o=>filter(FormatTableRouteWithDataSource(dataSourceName,o)))
                    .Select(tail=>new TableRouteUnit(dataSourceName,tail,typeof(TEntity)))
            ).ToList();

            return sqlRouteUnits;
        }


        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值, 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKey">分表的值</param>
        /// <param name="shardingOperator">操作</param>
        /// <param name="shardingPropertyName">分表字段</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        public virtual Func<string, bool> GetRouteFilter(object shardingKey,
            ShardingOperatorEnum shardingOperator, string shardingPropertyName)
        {
            if (EntityMetadata.IsMainShardingTableKey(shardingPropertyName))
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

        public override TableRouteUnit RouteWithValue(DataSourceRouteResult dataSourceRouteResult, object shardingKey)
        {
            if (dataSourceRouteResult.IntersectDataSources.Count !=1)
            {
                throw new ShardingCoreException($"more than one route match data source:{string.Join(",", dataSourceRouteResult.IntersectDataSources)}");
            }
            var shardingKeyToTail = ShardingKeyToTail(shardingKey);

            var filterTails = GetTails().Where(o => o == shardingKeyToTail).ToList();
            if (filterTails.IsEmpty())
            {
                throw new ShardingCoreException($"sharding key route not match {EntityMetadata.EntityType} -> [{EntityMetadata.ShardingTableProperty.Name}] -> [{shardingKey}] -> sharding key to tail :[{shardingKeyToTail}] ->  all tails ->[{string.Join(",", GetTails())}]");
            }

            if (filterTails.Count > 1)
                throw new ShardingCoreException($"more than one route match table:{string.Join(",", filterTails)}");
            return new TableRouteUnit(dataSourceRouteResult.IntersectDataSources.First(), filterTails[0],typeof(TEntity));
        }

    }
}