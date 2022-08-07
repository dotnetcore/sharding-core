using System;
using System.Collections.Generic;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.StreamMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 15:15:34
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal interface IStreamEnumerable<TEntity> : IAsyncEnumerable<TEntity>, IEnumerable<TEntity>, IDisposable
    {
    }
}
