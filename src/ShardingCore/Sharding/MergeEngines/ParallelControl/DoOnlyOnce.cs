using System.Threading;

namespace ShardingCore.Sharding.MergeEngines.ParallelControl
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 17 November 2020 16:23:53
* @Email: 326308290@qq.com
*/
    public class DoOnlyOnce
    {
        
        private const int Did = 1;
        private const int UnDo = 0;
        private int Status = UnDo;
        
        public bool IsUnDo()
        {
            if (Status == Did)
                return false;
            return Interlocked.CompareExchange(ref Status, Did, UnDo) == UnDo;
        }
    }
}