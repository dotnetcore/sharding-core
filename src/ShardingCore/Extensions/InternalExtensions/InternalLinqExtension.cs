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
        public static IOrderedEnumerable<TShource> ThenByIf<TShource, TKey>(this IOrderedEnumerable<TShource> source, Func<TShource, TKey> keySelector, bool condition,
            IComparer<TKey>? comparer)
        {
            return condition ? source.ThenBy(keySelector, comparer) : source;
        }
        public static IOrderedEnumerable<TShource> ThenByDescendingIf<TShource, TKey>(this IOrderedEnumerable<TShource> source, Func<TShource, TKey> keySelector, bool condition,
            IComparer<TKey>? comparer)
        {
            return condition ? source.ThenByDescending(keySelector, comparer) : source;
        }
    }
}
