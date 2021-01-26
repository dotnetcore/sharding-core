using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Helpers
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 17 December 2020 16:10:54
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 异步转同步,防止ASP.NET中死锁
    /// https://cpratt.co/async-tips-tricks/
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory =
            new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// 同步执行
        /// </summary>
        /// <param name="func">任务</param>
        public static void RunSync(Func<Task> func)
        {
            _myTaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 同步执行
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="func">任务</param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _myTaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }
    }
}