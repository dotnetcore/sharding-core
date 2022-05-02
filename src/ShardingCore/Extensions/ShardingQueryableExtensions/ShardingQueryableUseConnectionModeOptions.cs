using System;
using ShardingCore.Core;

namespace ShardingCore.Extensions.ShardingQueryableExtensions
{

    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 31 January 2022 22:51:56
    * @Email: 326308290@qq.com
    */
    public class ShardingQueryableUseConnectionModeOptions
    {
        public ShardingQueryableUseConnectionModeOptions(int maxQueryConnectionsLimit, ConnectionModeEnum connectionMode)
        {
            MaxQueryConnectionsLimit = maxQueryConnectionsLimit;
            ConnectionMode = connectionMode;
        }

        public int MaxQueryConnectionsLimit { get; }
        public ConnectionModeEnum ConnectionMode { get; }
    }
}