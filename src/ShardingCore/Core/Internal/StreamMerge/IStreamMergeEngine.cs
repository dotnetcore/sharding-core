using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;

namespace ShardingCore.Core.Internal.StreamMerge
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 29 January 2021 16:11:32
* @Email: 326308290@qq.com
*/
    internal interface  IStreamMergeEngine<T>:IDisposable
    {
        Task<IEnumerable<IStreamMergeAsyncEnumerator<T>>> GetStreamEnumerator();
    }
}