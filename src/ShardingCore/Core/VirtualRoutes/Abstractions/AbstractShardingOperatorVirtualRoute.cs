using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 19:55:24
* @Email: 326308290@qq.com
*/
    public abstract class AbstractShardingOperatorVirtualRoute<T, TKey> : AbstractVirtualRoute<T, TKey> where T : class, IShardingEntity
    {
        protected override List<IPhysicTable> DoRouteWithWhere(List<IPhysicTable> allPhysicTables, IQueryable queryable)
        {
            //获取所有需要路由的表后缀
            var filter = ShardingKeyUtil.GetRouteShardingTableFilter(queryable, ShardingKeyUtil.Parse(typeof(T)), ConvertToShardingKey, GetRouteToFilter);
            var physicTables = allPhysicTables.Where(o => filter(o.Tail)).ToList();
            return physicTables;
        }


        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值, 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKey">分表的值</param>
        /// <param name="shardingOperator">操作</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        protected abstract Expression<Func<string, bool>> GetRouteToFilter(TKey shardingKey, ShardingOperatorEnum shardingOperator);

        public override IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, object shardingKey)
        {
            var filter = GetRouteToFilter(ConvertToShardingKey(shardingKey), ShardingOperatorEnum.Equal).Compile();

            var physicTables = allPhysicTables.Where(o => filter(o.Tail)).ToList();
            if (physicTables.IsEmpty())
            {
                var routeConfig = ShardingKeyUtil.Parse(typeof(T));
                throw new ShardingKeyRouteNotMatchException($"{routeConfig.ShardingEntityType} -> [{routeConfig.ShardingField}] ->【{shardingKey}】");
            }

            if (physicTables.Count > 1)
                throw new ShardingKeyRouteMoreException($"table:{string.Join(",", physicTables.Select(o => $"[{o.FullName}]"))}");
            return physicTables[0];
        }
    }
}