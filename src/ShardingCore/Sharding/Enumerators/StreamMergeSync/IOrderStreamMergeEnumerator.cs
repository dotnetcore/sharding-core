using System;
using System.Collections.Generic;

namespace ShardingCore.Sharding.Enumerators.StreamMergeSync
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 15 August 2021 06:44:33
* @Email: 326308290@qq.com
*/
    public interface IOrderStreamMergeEnumerator<T>:IStreamMergeEnumerator<T>, IComparable<IOrderStreamMergeEnumerator<T>>
    {
        List<IComparable> GetCompares();
    }
}