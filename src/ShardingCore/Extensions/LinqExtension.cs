using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 01 January 2021 16:16:11
* @Email: 326308290@qq.com
*/
    public static class LinqExtension
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            return !source.IsEmpty();
        }
        public static bool IsIn<T>(this T thisValue, params T[] values)
        {
            return values.Contains(thisValue);
        }
        /// <summary>
        /// 给IEnumerable拓展ForEach方法
        /// </summary>
        /// <typeparam name="T">模型类</typeparam>
        /// <param name="iEnumberable">数据源</param>
        /// <param name="func">方法</param>
        public static void ForEach<T>(this IEnumerable<T> iEnumberable, Action<T> func)
        {
            foreach (var item in iEnumberable)
            {
                func(item);
            }
        }
        
    }
}