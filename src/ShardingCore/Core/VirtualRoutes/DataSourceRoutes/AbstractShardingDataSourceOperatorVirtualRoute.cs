using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 17:21:48
* @Email: 326308290@qq.com
*/
    public abstract class AbstractShardingDataSourceOperatorVirtualRoute<T, TKey> : AbstractDataSourceVirtualRoute<T, TKey> where T : class, IShardingDataSource
    {
        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值, 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKey">分表的值</param>
        /// <param name="shardingOperator">操作</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        protected abstract Expression<Func<string, bool>> GetRouteToFilter(TKey shardingKey, ShardingOperatorEnum shardingOperator);
        /// <summary>
        /// 通过iqueryable来解析本次路由到的具体数据源
        /// </summary>
        /// <param name="allShardingDataSourceConfigs"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected override List<string> DoRouteWithWhere(List<string> allShardingDataSourceConfigs, IQueryable queryable)
        {
            //获取所有需要路由的表后缀
            var filter = ShardingUtil.GetRouteDataSourceFilter(queryable, ShardingUtil.Parse(typeof(T)), ConvertToShardingKey, GetRouteToFilter);
            var physicTables = allShardingDataSourceConfigs.Where(o => filter(o)).ToList();
            return physicTables;
        }
        /// <summary>
        /// 根据具体值路由到对应的数据源
        /// </summary>
        /// <param name="allPhysicDataSources"></param>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        /// <exception cref="ShardingDataSourceRouteNotMatchException"></exception>
        /// <exception cref="ShardingDataSourceRouteMatchMoreException"></exception>
        public override string RouteWithValue(List<string> allPhysicDataSources, object shardingKey)
        {
            var filter = GetRouteToFilter(ConvertToShardingKey(shardingKey), ShardingOperatorEnum.Equal).Compile();

            var physicDataSources = allPhysicDataSources.Where(o => filter(o)).ToList();
            if (physicDataSources.IsEmpty())
            {
                var shardingEntityBaseType = ShardingUtil.Parse(typeof(T));
                throw new ShardingDataSourceRouteNotMatchException($"{shardingEntityBaseType.EntityType} -> [{shardingEntityBaseType.ShardingDataSourceField}] ->【{shardingKey}】");
            }

            if (physicDataSources.Count > 1)
                throw new ShardingDataSourceRouteMatchMoreException($"data source :{string.Join(",", physicDataSources.Select(o => $"[{o}]"))}");
            return physicDataSources[0];
        }
    }
}