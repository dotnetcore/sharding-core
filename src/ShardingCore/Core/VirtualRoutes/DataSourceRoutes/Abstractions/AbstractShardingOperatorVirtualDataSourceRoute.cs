using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
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
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class AbstractShardingOperatorVirtualDataSourceRoute<T, TKey> : AbstractShardingFilterVirtualDataSourceRoute<T, TKey> where T : class
    {
        protected override List<string> DoRouteWithPredicate(List<string> allDataSourceNames, IQueryable queryable)
        {
            //获取所有需要路由的表后缀
            var filter = ShardingUtil.GetRouteShardingTableFilter(queryable, EntityMetadata, ConvertToShardingKey, GetRouteToFilter,false);
            var dataSources = allDataSourceNames.Where(o => filter(o)).ToList();
            return dataSources;
        }


        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值, 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKey">分表的值</param>
        /// <param name="shardingOperator">操作</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        protected abstract Expression<Func<string, bool>> GetRouteToFilter(TKey shardingKey, ShardingOperatorEnum shardingOperator);

        public override string RouteWithValue(object shardingKey)
        {
            var allDataSourceNames = GetAllDataSourceNames();
            var shardingKeyToDataSource = ShardingKeyToDataSourceName(shardingKey);

            var dataSources = allDataSourceNames.Where(o => o== shardingKeyToDataSource).ToList();
            if (dataSources.IsEmpty())
            {
                throw new ShardingKeyRouteNotMatchException($"{EntityMetadata.EntityType} -> [{EntityMetadata.ShardingTableProperty.Name}] ->【{shardingKey}】 all data sources ->[{string.Join(",", allDataSourceNames.Select(o=>o))}]");
            }

            if (dataSources.Count > 1)
                throw new ShardingKeyRouteMoreException($"data source:{string.Join(",", dataSources.Select(o => $"[{o}]"))}");
            return dataSources[0];
        }

    }
}