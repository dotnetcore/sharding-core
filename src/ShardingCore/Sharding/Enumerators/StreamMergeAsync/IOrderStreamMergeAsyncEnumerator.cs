using System;
using System.Collections.Generic;

namespace ShardingCore.Sharding.Enumerators
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 15 August 2021 06:44:33
    * @Email: 326308290@qq.com
    */
    internal interface IOrderStreamMergeAsyncEnumerator<T>:IStreamMergeAsyncEnumerator<T>, IComparable<IOrderStreamMergeAsyncEnumerator<T>>
    {
        List<IComparable> GetCompares();
    }
}