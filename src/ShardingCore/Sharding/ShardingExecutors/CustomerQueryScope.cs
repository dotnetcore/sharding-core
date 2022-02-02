using System;
using ShardingCore.Core.QueryRouteManagers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using ShardingCore.Sharding.ShardingExecutors;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

/*
* @Author: xjm
* @Description:
* @Date: DATE TIME
* @Email: 326308290@qq.com
*/
namespace ShardingCore.ShardingExecutors
{
    internal class CustomerQueryScope:IDisposable
    {
        private readonly ShardingRouteScope _shardingRouteScope;
        private readonly CustomerReadWriteScope _customerReadWriteScope;
        private readonly bool _hasCustomerQuery;
        public CustomerQueryScope(ICompileParameter compileParameter)
        {
            _hasCustomerQuery = compileParameter.HasCustomerQuery();
            if (_hasCustomerQuery)
            {
                var asRoute = compileParameter.GetAsRoute();
                if ( asRoute!= null)
                {
                    var shardingRouteManager = ShardingContainer.GetService<IShardingRouteManager>();
                    _shardingRouteScope = shardingRouteManager.CreateScope();
                    asRoute.Invoke(shardingRouteManager.Current);
                }

                var readOnly = compileParameter.ReadOnly();
                if (readOnly.HasValue)
                {
                    _customerReadWriteScope = new CustomerReadWriteScope(compileParameter.GetShardingDbContext(), readOnly.Value);
                }
            }
        }
        public void Dispose()
        {
            if (_hasCustomerQuery)
            {
                _shardingRouteScope?.Dispose();
                _customerReadWriteScope?.Dispose();
            }
        }
    }
}