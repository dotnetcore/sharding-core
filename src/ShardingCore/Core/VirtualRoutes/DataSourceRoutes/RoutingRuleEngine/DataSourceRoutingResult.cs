using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RoutingRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/2 16:13:49
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DataSourceRoutingResult
    {
        public DataSourceRoutingResult(ISet<string> intersectConfigs)
        {
            IntersectConfigs = intersectConfigs;
        }
        /// <summary>
        /// 交集
        /// </summary>
        public ISet<string> IntersectConfigs { get; }
    }
}
