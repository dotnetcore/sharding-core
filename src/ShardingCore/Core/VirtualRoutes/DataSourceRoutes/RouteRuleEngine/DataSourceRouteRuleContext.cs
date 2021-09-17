using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 12:57:43
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分库路由上下文
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataSourceRouteRuleContext<T>
    {
        public ISet<Type> QueryEntities { get; }
        public DataSourceRouteRuleContext(IQueryable<T> queryable)
        {
            Queryable = queryable;
            QueryEntities = queryable.ParseQueryableRoute();
        }
        /// <summary>
        /// 查询条件
        /// </summary>
        public IQueryable<T> Queryable { get; }
    }
}
