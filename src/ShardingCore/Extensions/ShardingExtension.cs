using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ShardingCore.Core;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 28 December 2020 16:50:57
* @Email: 326308290@qq.com
*/
    public static class ShardingExtension
    {
        /// <summary>
        /// 转成sharding queryable
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IShardingQueryable<T> AsSharding<T>(this IQueryable<T> source)
        {
            return new ShardingQueryable<T>(source);
        }
        public static async Task<bool> ShardingAnyAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            return await new ShardingQueryable<T>(source.Where(predicate)).AnyAsync();
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<ShardingPagedResult<T>> ToShardingPageResultAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            //设置每次获取多少页
            var take = pageSize <= 0 ? 1 : pageSize;
            //设置当前页码最小1
            var index = pageIndex <= 0 ? 1 : pageIndex;
            //需要跳过多少页
            var skip = (index - 1) * take;
            //获取每次总记录数
            var count = await new ShardingQueryable<T>(source).CountAsync();
            
            //当数据库数量小于要跳过的条数就说明没数据直接返回不在查询list
            if (count <= skip)
                return new ShardingPagedResult<T>(new List<T>(0), count);
            //获取剩余条数
            int remainingCount = count - skip;
            //当剩余条数小于take数就取remainingCount
            var realTake = remainingCount < take ? remainingCount : take;
            var data = await new ShardingQueryable<T>(source.Skip(skip).Take(realTake)).ToListAsync();
            return new ShardingPagedResult<T>(data, count);
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ToShardingList<T>(this IQueryable<T> source)
        {
            return  new ShardingQueryable<T>(source).ToList();
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<T>> ToShardingListAsync<T>(this IQueryable<T> source)
        {
            return await new ShardingQueryable<T>(source).ToListAsync();
        }
        /// <summary>
        /// 第一条
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ShardingFirstOrDefault<T>(this IQueryable<T> source)
        {
            return  new ShardingQueryable<T>(source).FirstOrDefault();
        }
        /// <summary>
        /// 第一条
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> ShardingFirstOrDefaultAsync<T>(this IQueryable<T> source)
        {
            return await new ShardingQueryable<T>(source).FirstOrDefaultAsync();
        }
        /// <summary>
        /// 最大
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ShardingMax<T>(this IQueryable<T> source)
        {
            return  new ShardingQueryable<T>(source).Max();
        }
        /// <summary>
        /// 最大
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> ShardingMaxAsync<T>(this IQueryable<T> source)
        {
            return await new ShardingQueryable<T>(source).MaxAsync();
        }
        /// <summary>
        /// 最大
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult ShardingMax<T,TResult>(this IQueryable<T> source,Expression<Func<T,TResult>> keySelector)
        {
            return  ShardingMax<TResult>(source.Select(keySelector));
        }
        /// <summary>
        /// 最大
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> ShardingMaxAsync<T,TResult>(this IQueryable<T> source,Expression<Func<T,TResult>> keySelector)
        {
            return await ShardingMaxAsync<TResult>(source.Select(keySelector));
        }
        public static T ShardingMin<T>(this IQueryable<T> source)
        {
            return  new ShardingQueryable<T>(source).Min();
        }
        public static async Task<T> ShardingMinAsync<T>(this IQueryable<T> source)
        {
            return await new ShardingQueryable<T>(source).MinAsync();
        }
        /// <summary>
        /// 最小
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult ShardingMin<T,TResult>(this IQueryable<T> source,Expression<Func<T,TResult>> keySelector)
        {
            return  ShardingMin(source.Select(keySelector));
        }
        /// <summary>
        /// 最小
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> ShardingMinAsync<T,TResult>(this IQueryable<T> source,Expression<Func<T,TResult>> keySelector)
        {
            return await ShardingMinAsync(source.Select(keySelector));
        }
        public static int ShardingCount<T>(this IQueryable<T> source)
        {
            return new ShardingQueryable<T>(source).Count();
        }
        public static async Task<int> ShardingCountAsync<T>(this IQueryable<T> source)
        {
            return await new ShardingQueryable<T>(source).CountAsync();
        }
        public static long ShardingLongCount<T>(this IQueryable<T> source)
        {
            return  new ShardingQueryable<T>(source).LongCount();
        }
        public static async Task<long> ShardingLongCountAsync<T>(this IQueryable<T> source)
        {
            return await new ShardingQueryable<T>(source).LongCountAsync();
        }
        public static int ShardingSum(this IQueryable<int> source)
        {
            return  new ShardingQueryable<int>(source).Sum();
        }
        public static async Task<int> ShardingSumAsync(this IQueryable<int> source)
        {
            return await new ShardingQueryable<int>(source).SumAsync();
        }
        public static int ShardingSum<T>(this IQueryable<T> source,Expression<Func<T,int>> keySelector)
        {
            return  ShardingSum(source.Select(keySelector));
        }
        public static async Task<int> ShardingSumAsync<T>(this IQueryable<T> source,Expression<Func<T,int>> keySelector)
        {
            return await ShardingSumAsync(source.Select(keySelector));
        }
        public static long ShardingSum(this IQueryable<long> source)
        {
            return  new ShardingQueryable<long>(source).LongSum();
        }
        public static async Task<long> ShardingSumAsync(this IQueryable<long> source)
        {
            return await new ShardingQueryable<long>(source).LongSumAsync();
        }
        public static long ShardingSum<T>(this IQueryable<T> source,Expression<Func<T,long>> keySelector)
        {
            return  ShardingSum(source.Select(keySelector));
        }
        public static async Task<long> ShardingSumAsync<T>(this IQueryable<T> source,Expression<Func<T,long>> keySelector)
        {
            return await ShardingSumAsync(source.Select(keySelector));
        }
        public static double ShardingSum(this IQueryable<double> source)
        {
            return  new ShardingQueryable<double>(source).DoubleSum();
        }
        public static async Task<double> ShardingSumAsync(this IQueryable<double> source)
        {
            return await new ShardingQueryable<double>(source).DoubleSumAsync();
        }
        public static double ShardingSum<T>(this IQueryable<T> source,Expression<Func<T,double>> keySelector)
        {
            return  ShardingSum(source.Select(keySelector));
        }
        public static async Task<double> ShardingSumAsync<T>(this IQueryable<T> source,Expression<Func<T,double>> keySelector)
        {
            return await ShardingSumAsync(source.Select(keySelector));
        }
        public static decimal ShardingSum(this IQueryable<decimal> source)
        {
            return  new ShardingQueryable<decimal>(source).DecimalSum();
        }
        public static async Task<decimal> ShardingSumAsync(this IQueryable<decimal> source)
        {
            return await new ShardingQueryable<decimal>(source).DecimalSumAsync();
        }
        public static decimal ShardingSum<T>(this IQueryable<T> source,Expression<Func<T,decimal>> keySelector)
        {
            return  ShardingSum(source.Select(keySelector));
        }
        public static async Task<decimal> ShardingSumAsync<T>(this IQueryable<T> source,Expression<Func<T,decimal>> keySelector)
        {
            return await ShardingSumAsync(source.Select(keySelector));
        }
        public static float ShardingSum(this IQueryable<float> source)
        {
            return  new ShardingQueryable<float>(source).FloatSum();
        }
        public static async Task<float> ShardingSumAsync(this IQueryable<float> source)
        {
            return await new ShardingQueryable<float>(source).FloatSumAsync();
        }
        public static float ShardingSum<T>(this IQueryable<T> source,Expression<Func<T,float>> keySelector)
        {
            return  ShardingSum(source.Select(keySelector));
        }

        public static async Task<float> ShardingSumAsync<T>(this IQueryable<T> source,Expression<Func<T,float>> keySelector)
        {
            return await ShardingSumAsync(source.Select(keySelector));
        }

    }
}