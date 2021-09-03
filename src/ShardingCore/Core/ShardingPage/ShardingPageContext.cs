using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Core.ShardingPage
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 13:46:55
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingPageContext
    {
        public  ICollection<RouteQueryResult<long>> RouteQueryResults { get; }
        private ShardingPageContext()
        {
            RouteQueryResults = new LinkedList<RouteQueryResult<long>>();
        }
        public static ShardingPageContext Create()
        {
            return new ShardingPageContext();
        }
    }
}
