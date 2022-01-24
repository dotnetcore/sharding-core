using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Abstractions.ParallelExecutors
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
        bool IsTrip<TResult>(IEnumerable<TResult> results);
        /// <summary>
        /// 跳闸
        /// </summary>
        void Trip();

        void Register(Action afterTrip);

    }
}
