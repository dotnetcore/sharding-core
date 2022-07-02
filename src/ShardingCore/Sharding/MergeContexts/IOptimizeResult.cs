using System.Collections.Generic;
using ShardingCore.Core;

namespace ShardingCore.Sharding.MergeContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 02 March 2022 21:24:53
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 优化结果
    /// </summary>
    public interface IOptimizeResult
    {
        int GetMaxQueryConnectionsLimit();
        ConnectionModeEnum GetConnectionMode();
        bool IsSequenceQuery();
        bool SameWithTailComparer();
        IComparer<string> ShardingTailComparer();
    }
}