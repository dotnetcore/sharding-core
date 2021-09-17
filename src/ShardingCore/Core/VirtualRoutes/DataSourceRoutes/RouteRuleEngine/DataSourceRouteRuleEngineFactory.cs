using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public DataSourceRouteRuleContext<T> CreateContext<T>(IQueryable<T> queryable)
        {
            return new DataSourceRouteRuleContext<T>(queryable);
        }
        /// <summary>
        /// 路由到具体的物理数据源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="shardingDbContextType"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public DataSourceRouteResult Route<T>(Type shardingDbContextType, IQueryable<T> queryable)
        {
            var ruleContext = CreateContext<T>(queryable);
            return _dataSourceRouteRuleEngine.Route(shardingDbContextType,ruleContext);
        }
        /// <summary>
        /// 路由到具体的物理数据源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="shardingDbContextType"></param>
        /// <param name="ruleContext"></param>
        /// <returns></returns>
        public DataSourceRouteResult Route<T>(Type shardingDbContextType, DataSourceRouteRuleContext<T> ruleContext)
        {
            return _dataSourceRouteRuleEngine.Route(shardingDbContextType, ruleContext);
        }
    }
}
