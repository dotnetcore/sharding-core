using System;
using System.Threading.Tasks;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 14 August 2021 22:05:40
* @Email: 326308290@qq.com
*/
    public interface IStreamMergeEngine<T>
    {
        Task<IStreamMergeAsyncEnumerator<T>> GetAsyncEnumerator();
        Task<IStreamMergeEnumerator<T>> GetEnumerator();
    }
}