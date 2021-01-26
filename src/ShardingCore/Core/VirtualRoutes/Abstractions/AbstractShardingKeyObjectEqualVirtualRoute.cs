using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 16:00:15
* @Email: 326308290@qq.com
*/
    public abstract class AbstractShardingKeyObjectEqualVirtualRoute<T,TKey>: AbstractShardingOperatorVirtualRoute<T,TKey> where T : class, IShardingEntity
    {
        private readonly ILogger<AbstractShardingKeyObjectEqualVirtualRoute<T, TKey>> _logger;

        protected AbstractShardingKeyObjectEqualVirtualRoute(ILogger<AbstractShardingKeyObjectEqualVirtualRoute<T,TKey>> _logger)
        {
            this._logger = _logger;
        }
        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值,operate where的操作值 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKey">分表的值</param>
        /// <param name="shardingOperator">分表的类型</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        protected override Expression<Func<string, bool>> GetRouteToFilter(TKey shardingKey, ShardingOperatorEnum shardingOperator)
        {
            if (shardingOperator == ShardingOperatorEnum.Equal)
                return GetRouteEqualToFilter(shardingKey);
            _logger.LogWarning($"没有找到对应的匹配需要进行多表扫描:ShardingOperator:[{shardingOperator}] ");
            return s => true;
        }

        /// <summary>
        /// 如何路由到具体表 shardingKeyValue:分表的值, 返回结果:如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表
        /// </summary>
        /// <param name="shardingKey">分表的值</param>
        /// <returns>如果返回true表示返回该表 第一个参数 tail 第二参数是否返回该物理表</returns>
        protected abstract Expression<Func<string, bool>> GetRouteEqualToFilter(TKey shardingKey);
    }
}