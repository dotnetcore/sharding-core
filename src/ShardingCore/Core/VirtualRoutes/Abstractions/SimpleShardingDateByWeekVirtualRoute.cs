using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 20 December 2020 20:46:36
* @Email: 326308290@qq.com
*/
    public abstract class SimpleShardingDateByWeekVirtualRoute<T>:AbstractShardingOperatorVirtualRoute<T,long> where T:class,IShardingEntity
    {
        private readonly ILogger<SimpleShardingDateByWeekVirtualRoute<T>> _logger;

        public SimpleShardingDateByWeekVirtualRoute(ILogger<SimpleShardingDateByWeekVirtualRoute<T>> logger)
        {
            _logger = logger;
        }
        protected override long ConvertToShardingKey(object shardingKey)
        {
            return (long) shardingKey;
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            return GetWeekTableTail(ConvertToShardingKey(shardingKey));
        }
        private string GetWeekTableTail(long dateTimeL)
        {
            var dateTime = dateTimeL.ConvertLongToTime();
            var monday = dateTime.GetMonday();
            var sunday = monday.AddDays(6);
            return $"{monday:yyyyMM}{monday:dd}_{sunday:dd}";
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(long shardingKey, ShardingOperatorEnum shardingOperator)
        {
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.UnKnown:
                    _logger.LogWarning($"没有找到对应的匹配需要进行多表扫描:ShardingOperator:[{shardingOperator}]");
                    return tail => true;
                    //throw new NotSupportedException(xxxx);
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    //yyyyMMdd
                    return tail =>String.Compare(tail, GetWeekTableTail(shardingKey), StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                case ShardingOperatorEnum.LessThanOrEqual:
                    //yyyyMMdd
                    return tail =>String.Compare(tail, GetWeekTableTail(shardingKey), StringComparison.Ordinal) <= 0;
                case ShardingOperatorEnum.Equal:
                    //yyyyMMdd
                    return tail =>tail == GetWeekTableTail(shardingKey);
                case ShardingOperatorEnum.NotEqual:
                    //yyyyMMdd
                    return tail =>tail != GetWeekTableTail(shardingKey);
                default:
                    throw new ArgumentOutOfRangeException(nameof(shardingOperator), shardingOperator, null);
            }
        }
    }
}