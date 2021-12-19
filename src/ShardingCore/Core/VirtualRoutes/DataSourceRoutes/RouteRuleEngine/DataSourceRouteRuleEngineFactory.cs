using System;
using System.Collections.Generic;
using System.Linq;
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
    public class DataSourceRouteRuleEngineFactory<TShardingDbContext>: IDataSourceRouteRuleEngineFactory<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IDataSourceRouteRuleEngine<TShardingDbContext> _dataSourceRouteRuleEngine;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dataSourceRouteRuleEngine"></param>
        public DataSourceRouteRuleEngineFactory(IDataSourceRouteRuleEngine<TShardingDbContext> dataSourceRouteRuleEngine)
        {
            _dataSourceRouteRuleEngine = dataSourceRouteRuleEngine;
        }
        /// <summary>
        /// 通过表达式创建分库路由上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public DataSourceRouteRuleContext CreateContext(IQueryable queryable)
        {
            return new DataSourceRouteRuleContext(queryable,typeof(TShardingDbContext));
        }
        /// <summary>
        /// 路由到具体的物理数据源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public DataSourceRouteResult Route(IQueryable queryable)
        {
            var ruleContext = CreateContext(queryable);
            return _dataSourceRouteRuleEngine.Route(ruleContext);
        }
        /// <summary>
        /// 路由到具体的物理数据源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleContext"></param>
        /// <returns></returns>
        public DataSourceRouteResult Route(DataSourceRouteRuleContext ruleContext)
        {
            return _dataSourceRouteRuleEngine.Route(ruleContext);
        }
    }
}
