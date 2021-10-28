using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.Internal;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.Querys;
using ShardingCore.Core.VirtualDatabase;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Extensions;

namespace ShardingCore.Utils
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 06 February 2021 08:28:07
* @Email: 326308290@qq.com
*/
    public class ShardingUtil
    {
        
        /// <summary>
        /// 分库路由过滤
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="shardingEntityBaseType"></param>
        /// <param name="shardingKeyConvert"></param>
        /// <param name="keyToTailExpression"></param>
        /// <returns></returns>
        public static Func<string, bool> GetRouteDataSourceFilter<TKey>(IQueryable queryable, ShardingEntityConfig shardingEntityBaseType, Func<object, TKey> shardingKeyConvert, Func<TKey, ShardingOperatorEnum, Expression<Func<string, bool>>> keyToTailExpression)
        {
            QueryableRouteShardingDataSourceDiscoverVisitor<TKey> visitor = new QueryableRouteShardingDataSourceDiscoverVisitor<TKey>(shardingEntityBaseType, shardingKeyConvert, keyToTailExpression);

            visitor.Visit(queryable.Expression);

            return visitor.GetDataSourceFilter();
        }
        /// <summary>
        /// 分表路由过滤
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="entityMetadata"></param>
        /// <param name="shardingKeyConvert"></param>
        /// <param name="keyToTailExpression"></param>
        /// <returns></returns>
        public static Func<string, bool> GetRouteShardingTableFilter<TKey>(IQueryable queryable, EntityMetadata entityMetadata, Func<object, TKey> shardingKeyConvert, Func<TKey, ShardingOperatorEnum, Expression<Func<string, bool>>> keyToTailExpression)
        {
            QueryableRouteShardingTableDiscoverVisitor<TKey> visitor = new QueryableRouteShardingTableDiscoverVisitor<TKey>(entityMetadata, shardingKeyConvert, keyToTailExpression);

            visitor.Visit(queryable.Expression);

            return visitor.GetStringFilterTail();
        }
        /// <summary>
        /// 获取本次查询的所有涉及到的对象
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static ISet<Type> GetQueryEntitiesFilter(IQueryable queryable)
        {
            QueryEntitiesVisitor visitor = new QueryEntitiesVisitor();

            visitor.Visit(queryable.Expression);

            return visitor.GetQueryEntities();
        }

    }
}