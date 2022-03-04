using System;
using ShardingCore.Extensions.ShardingQueryableExtensions;

/*
* @Author: xjm
* @Description:
* @Date: DATE TIME
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Sharding.Visitors.ShardingExtractParameters
{
    internal class ShardingExtParameter
    {
        public bool UseUnionAllMerge { get; }
        public ShardingQueryableAsRouteOptions ShardingQueryableAsRouteOptions { get; }
        public ShardingQueryableUseConnectionModeOptions ShardingQueryableUseConnectionModeOptions { get; }
        public ShardingQueryableReadWriteSeparationOptions ShardingQueryableReadWriteSeparationOptions { get; }
        public ShardingQueryableAsSequenceOptions ShardingQueryableAsSequenceOptions { get; }

        public ShardingExtParameter(bool useUnionAllMerge,
            ShardingQueryableAsRouteOptions shardingQueryableAsRouteOptions,
            ShardingQueryableUseConnectionModeOptions shardingQueryableUseConnectionModeOptions,
            ShardingQueryableReadWriteSeparationOptions shardingQueryableReadWriteSeparationOptions,
            ShardingQueryableAsSequenceOptions shardingQueryableAsSequenceOptions)
        {
            UseUnionAllMerge = useUnionAllMerge;
            ShardingQueryableAsRouteOptions = shardingQueryableAsRouteOptions;
            ShardingQueryableUseConnectionModeOptions = shardingQueryableUseConnectionModeOptions;
            ShardingQueryableReadWriteSeparationOptions = shardingQueryableReadWriteSeparationOptions;
            ShardingQueryableAsSequenceOptions = shardingQueryableAsSequenceOptions;
        }
    }
}