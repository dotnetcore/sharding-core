using System;
using System.Linq;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 22 August 2021 09:46:07
* @Email: 326308290@qq.com
*/
    public class SingleQueryRouteTail:ISingleQueryRouteTail
    {
        private readonly TableRouteResult _tableRouteResult;
        private readonly string _tail;
        private readonly string _modelCacheKey;
        private readonly bool _isShardingTableQuery;

        public SingleQueryRouteTail(TableRouteResult tableRouteResult)
        {
            if (tableRouteResult.ReplaceTables.IsEmpty() || tableRouteResult.ReplaceTables.Count > 1) throw new ArgumentException("route result replace tables must 1");
            _tableRouteResult = tableRouteResult;
            _tail= _tableRouteResult.ReplaceTables.First().Tail;
            _modelCacheKey = _tail.FormatRouteTail2ModelCacheKey();
            _isShardingTableQuery = !string.IsNullOrWhiteSpace(_tail);
        }

        public SingleQueryRouteTail(string tail)
        {
            _tail= tail;
            _modelCacheKey = _tail.FormatRouteTail2ModelCacheKey();
            _isShardingTableQuery = !string.IsNullOrWhiteSpace(_tail);
        }
        public virtual string GetRouteTailIdentity()
        {
            return _modelCacheKey;
        }

        public virtual bool IsMultiEntityQuery()
        {
            return false;
        }

        public bool IsShardingTableQuery()
        {
            return _isShardingTableQuery;
        }

        public virtual string GetTail()
        {
            return _tail;
        }
    }
}