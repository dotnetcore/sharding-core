using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.VirtualRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.RouteTails
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 22 August 2021 09:59:22
* @Email: 326308290@qq.com
*/
    public class MultiQueryRouteTail:IMultiQueryRouteTail
    {
        private const string RANDOM_MODEL_CACHE_KEY = "RANDOM_MODEL_CACHE_KEY";
        private readonly RouteResult _routeResult;
        private readonly string _modelCacheKey;
        private readonly ISet<Type> _entityTypes;

        public MultiQueryRouteTail(RouteResult routeResult)
        {
            if (routeResult.ReplaceTables.IsEmpty() || routeResult.ReplaceTables.Count <= 1) throw new ArgumentException("route result replace tables must greater than  1");
            _routeResult = routeResult;
            _modelCacheKey = RANDOM_MODEL_CACHE_KEY+Guid.NewGuid().ToString("n");
            _entityTypes = routeResult.ReplaceTables.Select(o=>o.EntityType).ToHashSet();
        }
        public string GetRouteTailIdentity()
        {
            return _modelCacheKey;
        }

        public bool IsMultiEntityQuery()
        {
            return true;
        }

        public string GetEntityTail(Type entityType)
        {
            return _routeResult.ReplaceTables.Single(o => o.EntityType == entityType).Tail;
        }

        public ISet<Type> GetEntityTypes()
        {
            return _entityTypes;
        }
    }
}