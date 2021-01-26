using System;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 02 January 2021 13:10:30
* @Email: 326308290@qq.com
*/
    public static class DateTimeExtension
    {
        #region 13位时间戳

        /// <summary>  
        /// 将DateTime时间格式转换为本地时间戳格式
        /// </summary>
        /// Author  : Napoleon
        /// Created : 2018/4/8 14:19
        public static long ConvertTimeToLong(this DateTime time)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long) (time.AddHours(-8) - start).TotalMilliseconds;
        }

        /// <summary>        
        /// 本地时间戳转为C#格式时间
        /// </summary>
        /// Author  : Napoleon
        /// Created : 2018/4/8 14:19
        public static DateTime ConvertLongToTime(this long timeStamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddMilliseconds(timeStamp).AddHours(8);
        }

        #endregion


        /// <summary>
        /// 获取周一
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetMonday(this DateTime dateTime)
        {
            DateTime temp = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            int count = dateTime.DayOfWeek - DayOfWeek.Monday;
            if (count == -1) count = 6;
            var monday = temp.AddDays(-count);
            return monday;
        }
    }
}