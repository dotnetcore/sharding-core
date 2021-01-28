using System;

namespace ShardingCore.Core.Internal.RoutingRuleEngines
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:53:55
* @Email: 326308290@qq.com
*/
    public class RouteRuleConfig
    {
        private bool _autoParseRoute = true;

        public bool GetAutoParseRoute()
        {
            return _autoParseRoute;
        }

        /// <summary>
        /// 启用自动路由
        /// </summary>
        public void EnableAutoRouteParse()
        {
            _autoParseRoute = true;
        }

        /// <summary>
        /// 禁用自动路由
        /// </summary>
        public void DisableAutoRouteParse()
        {
            _autoParseRoute = false;
        }
    }
}