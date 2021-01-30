using System;
using System.Collections.Generic;

namespace ShardingCore.Core.Internal.StreamMerge.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 30 January 2021 15:27:48
* @Email: 326308290@qq.com
*/
    public interface IOrderStreamMergeAsyncEnumerator<T>:IStreamMergeAsyncEnumerator<T>, IComparable<IOrderStreamMergeAsyncEnumerator<T>>
    {
        List<IComparable> GetCompares();
    }
}