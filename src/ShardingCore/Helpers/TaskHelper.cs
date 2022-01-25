using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Helpers
{
    internal class TaskHelper
    {
        private TaskHelper()
        {
            throw new InvalidOperationException(nameof(TaskHelper));
        }
        public static Task<TResult[]> WhenAllFastFail<TResult>(params Task<TResult>[] tasks)
        {
            if (tasks is null || tasks.Length == 0) return Task.FromResult(Array.Empty<TResult>());
            // defensive copy.
            var defensive = tasks.Clone() as Task<TResult>[];

            var tcs = new TaskCompletionSource<TResult[]>();
            var remaining = defensive.Length;

            Action<Task> check = t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.Faulted:
                        // we 'try' as some other task may beat us to the punch.
                        tcs.TrySetException(t.Exception.InnerException);
                        break;
                    case TaskStatus.Canceled:
                        // we 'try' as some other task may beat us to the punch.
                        tcs.TrySetCanceled();
                        break;
                    default:

                        // we can safely set here as no other task remains to run.
                        if (Interlocked.Decrement(ref remaining) == 0)
                        {
                            // get the results into an array.
                            var results = new TResult[defensive.Length];
                            for (var i = 0; i < tasks.Length; ++i) results[i] = defensive[i].Result;
                            tcs.SetResult(results);
                        }
                        break;
                }
            };

            foreach (var task in defensive)
            {
                task.ContinueWith(check, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            return tcs.Task;
        }
    }
}
