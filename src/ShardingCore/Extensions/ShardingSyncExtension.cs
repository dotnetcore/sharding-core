using System.Collections.Generic;
using ShardingCore.Core;
using ShardingCore.Helpers;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 30 December 2020 15:22:12
* @Email: 326308290@qq.com
*/
    public static class ShardingSyncExtension
    {
        public static bool Any<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.AnyAsync());
        }

        public static T FirstOrDefault<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.FirstOrDefaultAsync());
        }

        public static List<T> ToList<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.ToListAsync());
        }

        public static int Count<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.CountAsync());
        }

        public static long LongCount<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.LongCountAsync());
        }

        public static int Sum<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.SumAsync());
        }

        public static double DoubleSum<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.DoubleSumAsync());
        }

        public static decimal DecimalSum<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.DecimalSumAsync());
        }

        public static long LongSum<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.LongSumAsync());
        }

        public static float FloatSum<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.FloatSumAsync());
        }

        public static T Max<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.MaxAsync());
        }

        public static T Min<T>(this IShardingQueryable<T> queryable)
        {
            return AsyncHelper.RunSync(() => queryable.MinAsync());
        }
    }
}