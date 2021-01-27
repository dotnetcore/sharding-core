using System;

namespace ShardingCore.Helpers
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 22 January 2021 13:32:08
* @Email: 326308290@qq.com
*/
    public class ShardingCoreHelper
    {
        private ShardingCoreHelper(){}
        public static int GetStringHashCode(string value)
        {
            int h = 0; // 默认值是0
            if (value.Length > 0) {
                for (int i = 0; i < value.Length; i++) {
                    h = 31 * h + value[i]; // val[0]*31^(n-1) + val[1]*31^(n-2) + ... + val[n-1]
                }
            }
            return h;
        }

        private static readonly DateTime UtcStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime ConvertLongToDateTime(long timeStamp)
        {
            return UtcStartTime.AddMilliseconds(timeStamp).AddHours(8);
        }

        /// <summary>
        /// 获取当月第一天
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetCurrentMonthFirstDay(DateTime time)
        {
            return time.AddDays(1 - time.Day).Date;
        }
        /// <summary>
        /// 获取下个月第一天
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetNextMonthFirstDay(DateTime time)
        {
            return time.AddDays(1 - time.Day).Date.AddMonths(1);
        }

        public static DateTime GetCurrentMonday(DateTime time)
        {
            DateTime dateTime1 = new DateTime(time.Year, time.Month, time.Day);
            int num = (int) (time.DayOfWeek - 1);
            if (num == -1)
                num = 6;
            return dateTime1.AddDays(-num);
        }
        public static DateTime GetCurrentSunday(DateTime time)
        {
            return GetCurrentMonday(time).AddDays(6);
        }
    }
}