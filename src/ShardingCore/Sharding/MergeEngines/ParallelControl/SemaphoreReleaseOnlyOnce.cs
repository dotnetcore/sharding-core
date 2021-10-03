using System.Threading;

namespace ShardingCore.Sharding.MergeEngines.ParallelControl
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 17 November 2020 13:43:39
* @Email: 326308290@qq.com
*/
    public class SemaphoreReleaseOnlyOnce
    {
        private readonly DoOnlyOnce _doOnlyOnce=new DoOnlyOnce();
        private readonly SemaphoreSlim _semaphore;
        
        public SemaphoreReleaseOnlyOnce(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Release()
        {
            if (_semaphore != null)
            {
                if (_doOnlyOnce.IsUnDo())
                {
                    _semaphore.Release();
                }
            }
        }
    }
}