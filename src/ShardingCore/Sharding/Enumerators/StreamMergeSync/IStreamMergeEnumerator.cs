using System.Collections.Generic;

namespace ShardingCore.Sharding.Enumerators.StreamMergeSync
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 14 August 2021 22:06:38
* @Email: 326308290@qq.com
*/
    public interface IStreamMergeEnumerator<T>:IEnumerator<T>
    {
        bool SkipFirst();
        bool HasElement();
        T ReallyCurrent { get; }
    }
}