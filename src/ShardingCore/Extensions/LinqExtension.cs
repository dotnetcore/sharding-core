using System;
using System.Collections;
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

        //public static void ForEach<T>(this IEnumerable<T> source)

#if !EFCORE5
        public static HashSet<TSource> ToHashSet<TSource>(
            this IEnumerable<TSource> source,
            IEqualityComparer<TSource> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new HashSet<TSource>(source, comparer);
        }
#endif
        /// <summary>
        /// 求集合的笛卡尔积
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Cartesian<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> tempProduct = new[] {Enumerable.Empty<T>()};
            return sequences.Aggregate(tempProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] {item})
            );
        }
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            return !source.IsEmpty();
        }

        private static readonly HashSet<string> _enumerableContainsNamespace = new HashSet<string>()
        {
            "System.Linq", "System.Collections.Generic"
        };
        public static bool IsInEnumerable(this string thisValue)
        {
            return _enumerableContainsNamespace.Contains(thisValue);
        }
        /// <summary>
        /// 按size分区,每个区size个数目
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="elements"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<List<TSource>> Partition<TSource>(this IEnumerable<TSource> elements,int size)
        {
           return elements.Select((o, i) => new { Element = o, Index = i / size })
                .GroupBy(o => o.Index).Select(o => o.Select(g => g.Element).ToList());
        }

    }
}