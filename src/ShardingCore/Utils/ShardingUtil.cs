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
using ShardingCore.Sharding.Visitors.Querys;

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
        /// 获取表达式路由
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="entityMetadata"></param>
        /// <param name="keyToTailExpression"></param>
        /// <param name="shardingTableRoute">sharding table or data source</param>
        /// <returns></returns>
        public static RoutePredicateExpression GetRouteParseExpression(IQueryable queryable, EntityMetadata entityMetadata, Func<object, ShardingOperatorEnum,string, Func<string, bool>> keyToTailExpression,bool shardingTableRoute)
        {

            QueryableRouteShardingTableDiscoverVisitor visitor = new QueryableRouteShardingTableDiscoverVisitor(entityMetadata, keyToTailExpression, shardingTableRoute);

            visitor.Visit(queryable.Expression);

            return visitor.GetRouteParseExpression();
        }

    }
}