using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.VirtualDatabase;

namespace ShardingCore.Extensions
{
    public static class ShardingEntityConfigExtension
    {
        public static bool ShardingDataSourceFieldIsKey(this ShardingEntityConfig shardingEntityConfig)
        {
            if (string.IsNullOrWhiteSpace(shardingEntityConfig.ShardingDataSourceField))
                return false;
            return shardingEntityConfig.ShardingDataSourceField == shardingEntityConfig.SinglePrimaryKeyFieldName;
        }
        public static bool ShardingTableFieldIsKey(this ShardingEntityConfig shardingEntityConfig)
        {
            if (string.IsNullOrWhiteSpace(shardingEntityConfig.ShardingTableField))
                return false;
            return shardingEntityConfig.ShardingTableField == shardingEntityConfig.SinglePrimaryKeyFieldName;
        }
    }
}
