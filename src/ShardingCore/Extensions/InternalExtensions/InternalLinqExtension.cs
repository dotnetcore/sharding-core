using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ShardingCore.Extensions.InternalExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 10:13:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal static class InternalLinqExtension
    {
        public static IEnumerable<TShource> OrderByIf<TShource, TKey>(this IEnumerable<TShource> source, Func<TShource, TKey> keySelector, bool condition,
            IComparer<TKey>? comparer)
        {
            return condition ? source.OrderBy(keySelector, comparer) : source;
        }
        public static IEnumerable<TShource> OrderByDescendingIf<TShource, TKey>(this IEnumerable<TShource> source, Func<TShource, TKey> keySelector, bool condition,
            IComparer<TKey>? comparer)
        {
            return condition ? source.OrderByDescending(keySelector, comparer) : source;
        }
    }
}
