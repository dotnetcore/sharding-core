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
    public interface IOrderStreamMergeAsyncEnumerator<T>:IStreamMergeAsyncEnumerator<T>
    {
        List<IComparable> GetCompares();
    }
}