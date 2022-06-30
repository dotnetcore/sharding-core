using System;
using System.Collections.Generic;

namespace ShardingCore.Sharding.MergeEngines.Executors.Abstractions
{
    /// <summary>
    /// 断路器
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// 是否拉闸表示是否终端
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="results"></param>
        /// <returns></returns>
        bool Terminated<TResult>(IEnumerable<TResult> results);
        /// <summary>
        /// 跳闸
        /// </summary>
        void Terminated0();

        void Register(Action afterTrip);

    }
}
