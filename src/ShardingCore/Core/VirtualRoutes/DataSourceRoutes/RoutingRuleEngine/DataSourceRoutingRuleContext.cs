using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.VirtualDataSources;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RoutingRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/2 16:17:05
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DataSourceRoutingRuleContext<T>
    {

        public DataSourceRoutingRuleContext(IQueryable<T> queryable)
        {
            Queryable = queryable;
        }
        /// <summary>
        /// 查询条件
        /// </summary>
        public IQueryable<T> Queryable { get; }
    }
}