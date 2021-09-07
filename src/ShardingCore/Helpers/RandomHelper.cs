using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ShardingCore.Helpers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 14:33:52
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public  static class RandomHelper
    {
        static int seed = Environment.TickCount;

        static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Next()
        {
            return random.Value.Next();
        }
        public static int Next(int max)
        {
            return random.Value.Next(max);
        }
        public static int Next(int min,int max)
        {
            return random.Value.Next(min,max);
        }
    }
}
