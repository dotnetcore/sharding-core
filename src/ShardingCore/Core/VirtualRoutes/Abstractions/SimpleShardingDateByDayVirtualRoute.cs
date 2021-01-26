using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 13:47:54
* @Email: 326308290@qq.com
*/
    public abstract class SimpleShardingDateByDayVirtualRoute<T>:AbstractShardingOperatorVirtualRoute<T,long> where T:class,IShardingEntity
    {
        private readonly ILogger<SimpleShardingDateByDayVirtualRoute<T>> _logger;

        protected SimpleShardingDateByDayVirtualRoute(ILogger<SimpleShardingDateByDayVirtualRoute<T>> logger)
        {
            _logger = logger;
        }
        protected override long ConvertToShardingKey(object shardingKey)
        {
            return (long) shardingKey;
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            return ConvertToShardingKey(shardingKey).ConvertLongToTime().ToString("yyyyMMdd");
        }


        protected override Expression<Func<string, bool>> GetRouteToFilter(long shardingKey, ShardingOperatorEnum shardingOperator)
        {
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.UnKnown:
                    _logger.LogWarning($"没有找到对应的匹配需要进行多表扫描:ShardingOperator:[{shardingOperator}]");
                    return tail => true;
                    //throw new NotSupportedException(xxxx);
                    break;
                case ShardingOperatorEnum.GreaterThan:
                    return tail =>int.Parse(tail) > int.Parse(shardingKey.ConvertLongToTime().ToString("yyyyMMdd"));
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    //yyyyMMdd
                    return tail =>int.Parse(tail) >= int.Parse(shardingKey.ConvertLongToTime().ToString("yyyyMMdd"));
                    break;
                case ShardingOperatorEnum.LessThan:
                    return tail =>int.Parse(tail) < int.Parse(shardingKey.ConvertLongToTime().ToString("yyyyMMdd"));
                case ShardingOperatorEnum.LessThanOrEqual:
                    //yyyyMMdd
                    return tail =>int.Parse(tail) <= int.Parse(shardingKey.ConvertLongToTime().ToString("yyyyMMdd"));
                    break;
                case ShardingOperatorEnum.Equal:
                    //yyyyMMdd
                    return tail =>int.Parse(tail) == int.Parse(shardingKey.ConvertLongToTime().ToString("yyyyMMdd"));
                    break;
                case ShardingOperatorEnum.NotEqual:
                    //yyyyMMdd
                    return tail =>int.Parse(tail) != int.Parse(shardingKey.ConvertLongToTime().ToString("yyyyMMdd"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shardingOperator), shardingOperator, null);
            }
        }


    }
}