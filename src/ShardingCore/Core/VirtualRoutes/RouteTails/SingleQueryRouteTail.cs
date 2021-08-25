using System;
using System.Linq;
using ShardingCore.Core.VirtualRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.RouteTails
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 22 August 2021 09:46:07
* @Email: 326308290@qq.com
*/
    public class SingleQueryRouteTail:ISingleQueryRouteTail
    {
        private readonly RouteResult _routeResult;
        private readonly string _tail;
        private readonly string _modelCacheKey;

        public SingleQueryRouteTail(RouteResult routeResult)
        {
            if (routeResult.ReplaceTables.IsEmpty() || routeResult.ReplaceTables.Count > 1) throw new ArgumentException("route result replace tables must 1");
            _routeResult = routeResult;
            _tail= _routeResult.ReplaceTables.First().Tail;
            _modelCacheKey = _tail.FormatRouteTail2ModelCacheKey();
        }

        public SingleQueryRouteTail(string tail)
        {
            _tail= tail;
            _modelCacheKey = _tail.FormatRouteTail2ModelCacheKey();
        }
        public virtual string GetRouteTailIdenty()
        {
            return _modelCacheKey;
        }

        public virtual bool IsMultiEntityQuery()
        {
            return false;
        }

        public virtual string GetTail()
        {
            return _tail;
        }
    }
}