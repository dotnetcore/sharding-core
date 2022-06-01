using ShardingCore.Core.PhysicTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

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
        protected override List<IPhysicTable> DoRouteWithPredicate(List<IPhysicTable> allPhysicTables, IQueryable queryable)
        {
            //获取路由后缀表达式
            var routeParseExpression = ShardingUtil.GetRouteParseExpression(queryable, EntityMetadata, GetRouteFilter,true);
            //表达式缓存编译
            // var filter =CachingCompile(routeParseExpression);
            var filter =routeParseExpression.GetRoutePredicate();
            //通过编译结果进行过滤
            var physicTables = allPhysicTables.Where(o => filter(o.Tail)).ToList();
            return physicTables;
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

        public override IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, object shardingKey)
        {
            var shardingKeyToTail = ShardingKeyToTail(shardingKey);

            var physicTables = allPhysicTables.Where(o => o.Tail== shardingKeyToTail).ToList();
            if (physicTables.IsEmpty())
            {
                throw new ShardingCoreException($"sharding key route not match {EntityMetadata.EntityType} -> [{EntityMetadata.ShardingTableProperty.Name}] ->【{shardingKey}】 all tails ->[{string.Join(",", allPhysicTables.Select(o=>o.FullName))}]");
            }

            if (physicTables.Count > 1)
                throw new ShardingCoreException($"more than one route match table:{string.Join(",", physicTables.Select(o => $"[{o.FullName}]"))}");
            return physicTables[0];
        }

    }
}