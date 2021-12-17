using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.Internal;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.Querys;
using ShardingCore.Core.TrackerManagers;
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

        ///// <summary>
        ///// 分库路由过滤
        ///// </summary>
        ///// <typeparam name="TKey"></typeparam>
        ///// <param name="queryable"></param>
        ///// <param name="entityMetadata"></param>
        ///// <param name="shardingKeyConvert"></param>
        ///// <param name="keyToTailExpression"></param>
        ///// <returns></returns>
        //public static Func<string, bool> GetRouteDataSourceFilter<TKey>(IQueryable queryable, EntityMetadata entityMetadata, Func<object, TKey> shardingKeyConvert, Func<TKey, ShardingOperatorEnum, Expression<Func<string, bool>>> keyToTailExpression)
        //{
        //    QueryableRouteShardingDataSourceDiscoverVisitor<TKey> visitor = new QueryableRouteShardingDataSourceDiscoverVisitor<TKey>(entityMetadata, shardingKeyConvert, keyToTailExpression);

        //    visitor.Visit(queryable.Expression);

        //    return visitor.GetDataSourceFilter();
        //}
        /// <summary>
        /// 分表路由过滤
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="entityMetadata"></param>
        /// <param name="keyToTailExpression"></param>
        /// <param name="shardingTableRoute">sharding table or data source</param>
        /// <returns></returns>
        public static Expression<Func<string, bool>> GetRouteParseExpression<TKey>(IQueryable queryable, EntityMetadata entityMetadata, Func<TKey, ShardingOperatorEnum, Expression<Func<string, bool>>> keyToTailExpression,bool shardingTableRoute)
        {

            QueryableRouteShardingTableDiscoverVisitor<TKey> visitor = new QueryableRouteShardingTableDiscoverVisitor<TKey>(entityMetadata, keyToTailExpression, shardingTableRoute);

            visitor.Visit(queryable.Expression);

            return visitor.GetRouteParseExpression();
        }
        /// <summary>
        /// 获取本次查询的所有涉及到的对象
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="dbContextType"></param>
        /// <returns></returns>
        public static ISet<Type> GetQueryEntitiesFilter(IQueryable queryable,Type dbContextType)
        {
            return GetQueryEntitiesByExpression(queryable.Expression, dbContextType);
        }
        public static ISet<Type> GetQueryEntitiesByExpression(Expression expression, Type dbContextType)
        {
            var trackerManager = (ITrackerManager)ShardingContainer.GetService(typeof(ITrackerManager<>).GetGenericType0(dbContextType));

            QueryEntitiesVisitor visitor = new QueryEntitiesVisitor(trackerManager);

            visitor.Visit(expression);

            return visitor.GetQueryEntities();
        }

    }
}