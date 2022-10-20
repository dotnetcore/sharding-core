using System;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Parsers.Abstractions;

/*
* @Author: xjm
* @Description:
* @Date: DATE TIME
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Sharding.ShardingExecutors
{
    internal class CustomerQueryScope:IDisposable
    {
        private readonly ShardingRouteScope _shardingRouteScope;
        private readonly bool _hasCustomerQuery;
        public CustomerQueryScope(IPrepareParseResult prepareParseResult,IShardingRouteManager shardingRouteManager)
        {
            _hasCustomerQuery = prepareParseResult.HasCustomerQuery();
            if (_hasCustomerQuery)
            {
                var asRoute = prepareParseResult.GetAsRoute();
                if ( asRoute!= null)
                {
                    _shardingRouteScope = shardingRouteManager.CreateScope();
                    asRoute.Invoke(shardingRouteManager.Current);
                }

            }
        }
        public void Dispose()
        {
            if (_hasCustomerQuery)
            {
                _shardingRouteScope?.Dispose();
            }
        }
    }
}