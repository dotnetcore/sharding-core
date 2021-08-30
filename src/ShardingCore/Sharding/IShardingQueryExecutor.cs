using System;
using System.Linq;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 27 August 2021 22:49:22
* @Email: 326308290@qq.com
*/
    public interface IShardingQueryExecutor<T>
    {
        IStreamMergeAsyncEnumerator<T> GetStreamMergeEnumerator();
    }
}