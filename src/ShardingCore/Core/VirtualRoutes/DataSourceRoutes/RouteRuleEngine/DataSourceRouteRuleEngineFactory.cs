using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 14:30:57
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分库路由引擎工程
    /// </summary>
    public class DataSourceRouteRuleEngineFactory: IDataSourceRouteRuleEngineFactory
    {
        private readonly IDataSourceRouteRuleEngine _dataSourceRouteRuleEngine;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dataSourceRouteRuleEngine"></param>
        public DataSourceRouteRuleEngineFactory(IDataSourceRouteRuleEngine dataSourceRouteRuleEngine)
        {
            _dataSourceRouteRuleEngine = dataSourceRouteRuleEngine;
        }
        /// <summary>
        /// 通过表达式创建分库路由上下文
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="shardingDbContext"></param>
        /// <param name="queryEntities"></param>
        /// <returns></returns>
        private DataSourceRouteRuleContext CreateContext(IQueryable queryable,IShardingDbContext shardingDbContext,Dictionary<Type,IQueryable> queryEntities)
        {
            return new DataSourceRouteRuleContext(queryable, shardingDbContext, queryEntities);
        }
        /// <summary>
        /// 路由到具体的物理数据源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="shardingDbContext"></param>
        /// <param name="queryEntities"></param>
        /// <returns></returns>
        public DataSourceRouteResult Route(IQueryable queryable, IShardingDbContext shardingDbContext, Dictionary<Type, IQueryable> queryEntities)
        {
            var ruleContext = CreateContext(queryable, shardingDbContext,queryEntities);
            return _dataSourceRouteRuleEngine.Route(ruleContext);
        }
        ///// <summary>
        ///// 路由到具体的物理数据源
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="ruleContext"></param>
        ///// <returns></returns>
        //public DataSourceRouteResult Route(DataSourceRouteRuleContext ruleContext)
        //{
        //    return _dataSourceRouteRuleEngine.Route(ruleContext);
        //}
    }
}
