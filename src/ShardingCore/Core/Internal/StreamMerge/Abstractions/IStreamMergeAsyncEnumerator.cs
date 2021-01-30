using System;
using System.Collections.Generic;

namespace ShardingCore.Core.Internal.StreamMerge.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 19:44:52
* @Email: 326308290@qq.com
*/
    public interface IStreamMergeAsyncEnumerator<T>:IAsyncEnumerator<T>
    {
        bool SkipFirst();
        bool HasElement();
        T ReallyCurrent { get; }
    }
}