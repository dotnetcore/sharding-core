using System;

namespace ShardingCore.Jobs
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 07 January 2021 13:01:53
* @Email: 326308290@qq.com
*/
    internal class UtcTime
    {
        private UtcTime(){}
        private static readonly long UtcStartTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        public static long CurrentTimeMillis()
        {
            return (DateTime.UtcNow.Ticks - UtcStartTicks) / 10000;
        }

        public static long InputUtcTimeMillis(DateTime utcTime)
        {
            return (utcTime.Ticks - UtcStartTicks) / 10000;
        }
    }
}