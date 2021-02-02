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
            return ShardingQueryable<T>.Create(source);
        }
        public static async Task<bool> ShardingAnyAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            return await ShardingQueryable<T>.Create(source.Where(predicate)).AnyAsync();
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<ShardingPagedResult<T>> ShardingPageResultAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            //设置每次获取多少页
            var take = pageSize <= 0 ? 1 : pageSize;
            //设置当前页码最小1
            var index = pageIndex <= 0 ? 1 : pageIndex;
            //需要跳过多少页
            var skip = (index - 1) * take;
            //获取每次总记录数
            var count = await ShardingQueryable<T>.Create(source).CountAsync();
            
            //当数据库数量小于要跳过的条数就说明没数据直接返回不在查询list
            if (count <= skip)
                return new ShardingPagedResult<T>(new List<T>(0), count);
            //获取剩余条数
            int remainingCount = count - skip;
            //当剩余条数小于take数就取remainingCount
            var realTake = remainingCount < take ? remainingCount : take;
            var data = await ShardingQueryable<T>.Create(source.Skip(skip).Take(realTake)).ToListAsync(realTake);
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
            return  ShardingQueryable<T>.Create(source).ToList();
        }
        /// <summary>
        /// 集合
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<T>> ToShardingListAsync<T>(this IQueryable<T> source)
        {
            return await ShardingQueryable<T>.Create(source).ToListAsync();
        }
        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<TElement>> ShardingGroupByAsync<T,TKey,TElement>(this IQueryable<T> source,
            Expression<Func<T, TKey>> keySelector,
            Expression<Func<IGrouping<TKey,T>,TElement>> elementSelector)
        {
            return (await ShardingQueryable<T>.Create(source.GroupBy(keySelector).Select(elementSelector)).ToListAsync());
        }
        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<TElement> ShardingGroupBy<T,TKey,TElement>(this IQueryable<T> source,
            Expression<Func<T, TKey>> keySelector,
            Expression<Func<IGrouping<TKey,T>,TElement>> elementSelector)
        {
            return ShardingQueryable<T>.Create(source.GroupBy(keySelector).Select(elementSelector)).ToList();
        }
        /// <summary>
        /// 第一条
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ShardingFirstOrDefault<T>(this IQueryable<T> source)
        {
            return  ShardingQueryable<T>.Create(source).FirstOrDefault();
        }
        /// <summary>
        /// 第一条
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> ShardingFirstOrDefaultAsync<T>(this IQueryable<T> source)
        {
            return await ShardingQueryable<T>.Create(source).FirstOrDefaultAsync();
        }
        /// <summary>
        /// 最大
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ShardingMax<T>(this IQueryable<T> source)
        {
            return  ShardingQueryable<T>.Create(source).Max();
        }
        /// <summary>
        /// 最大
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> ShardingMaxAsync<T>(this IQueryable<T> source)
        {
            return await ShardingQueryable<T>.Create(source).MaxAsync();
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
            return  ShardingQueryable<T>.Create(source).Min();
        }
        public static async Task<T> ShardingMinAsync<T>(this IQueryable<T> source)
        {
            return await ShardingQueryable<T>.Create(source).MinAsync();
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
            return ShardingQueryable<T>.Create(source).Count();
        }
        public static async Task<int> ShardingCountAsync<T>(this IQueryable<T> source)
        {
            return await ShardingQueryable<T>.Create(source).CountAsync();
        }
        public static long ShardingLongCount<T>(this IQueryable<T> source)
        {
            return  ShardingQueryable<T>.Create(source).LongCount();
        }
        public static async Task<long> ShardingLongCountAsync<T>(this IQueryable<T> source)
        {
            return await ShardingQueryable<T>.Create(source).LongCountAsync();
        }
        public static int ShardingSum(this IQueryable<int> source)
        {
            return  ShardingQueryable<int>.Create(source).Sum();
        }
        public static async Task<int> ShardingSumAsync(this IQueryable<int> source)
        {
            return await ShardingQueryable<int>.Create(source).SumAsync();
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
            return  ShardingQueryable<long>.Create(source).LongSum();
        }
        public static async Task<long> ShardingSumAsync(this IQueryable<long> source)
        {
            return await ShardingQueryable<long>.Create(source).LongSumAsync();
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
            return ShardingQueryable<double>.Create(source).DoubleSum();
        }
        public static async Task<double> ShardingSumAsync(this IQueryable<double> source)
        {
            return await ShardingQueryable<double>.Create(source).DoubleSumAsync();
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
            return  ShardingQueryable<decimal>.Create(source).DecimalSum();
        }
        public static async Task<decimal> ShardingSumAsync(this IQueryable<decimal> source)
        {
            return await ShardingQueryable<decimal>.Create(source).DecimalSumAsync();
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
            return  ShardingQueryable<float>.Create(source).FloatSum();
        }
        public static async Task<float> ShardingSumAsync(this IQueryable<float> source)
        {
            return await ShardingQueryable<float>.Create(source).FloatSumAsync();
        }
        public static float ShardingSum<T>(this IQueryable<T> source,Expression<Func<T,float>> keySelector)
        {
            return  ShardingSum(source.Select(keySelector));
        }

        public static async Task<float> ShardingSumAsync<T>(this IQueryable<T> source,Expression<Func<T,float>> keySelector)
        {
            return await ShardingSumAsync(source.Select(keySelector));
        }
        
        
        public static async Task<double> ShardingAverageAsync(this IQueryable<int> source)
        {
            return await ShardingQueryable<double>.Create(source).AverageAsync();
        }
        public static  double ShardingAverage(this IQueryable<int> source)
        {
            return  ShardingQueryable<double>.Create(source).Average();
        }
        public static double ShardingAverage<T>(this IQueryable<T> source,Expression<Func<T,int>> keySelector)
        {
            return  ShardingAverage(source.Select(keySelector));
        }

        public static async Task<double> ShardingAverageAsync<T>(this IQueryable<T> source,Expression<Func<T,int>> keySelector)
        {
            return await ShardingAverageAsync(source.Select(keySelector));
        }
        
        
        public static async Task<double> ShardingAverageAsync(this IQueryable<long> source)
        {
            return await ShardingQueryable<double>.Create(source).AverageAsync();
        }
        public static  double ShardingAverage(this IQueryable<long> source)
        {
            return  ShardingQueryable<double>.Create(source).Average();
        }
        public static double ShardingAverage<T>(this IQueryable<T> source,Expression<Func<T,long>> keySelector)
        {
            return  ShardingAverage(source.Select(keySelector));
        }

        public static async Task<double> ShardingAverageAsync<T>(this IQueryable<T> source,Expression<Func<T,long>> keySelector)
        {
            return await ShardingAverageAsync(source.Select(keySelector));
        }
        
        
        public static async Task<double> ShardingAverageAsync(this IQueryable<double> source)
        {
            return await ShardingQueryable<double>.Create(source).AverageAsync();
        }
        public static  double ShardingAverage(this IQueryable<double> source)
        {
            return  ShardingQueryable<double>.Create(source).Average();
        }
        public static double ShardingAverage<T>(this IQueryable<T> source,Expression<Func<T,double>> keySelector)
        {
            return  ShardingAverage(source.Select(keySelector));
        }

        public static async Task<double> ShardingAverageAsync<T>(this IQueryable<T> source,Expression<Func<T,double>> keySelector)
        {
            return await ShardingAverageAsync(source.Select(keySelector));
        }
        
        
        public static async Task<decimal> ShardingAverageAsync(this IQueryable<decimal> source)
        {
            return await ShardingQueryable<decimal>.Create(source).DecimalAverageAsync();
        }
        public static  decimal ShardingAverage(this IQueryable<decimal> source)
        {
            return  ShardingQueryable<decimal>.Create(source).DecimalAverage();
        }
        public static decimal ShardingAverage<T>(this IQueryable<T> source,Expression<Func<T,decimal>> keySelector)
        {
            return  ShardingAverage(source.Select(keySelector));
        }

        public static async Task<decimal> ShardingAverageAsync<T>(this IQueryable<T> source,Expression<Func<T,decimal>> keySelector)
        {
            return await ShardingAverageAsync(source.Select(keySelector));
        }
        
        public static async Task<float> ShardingAverageAsync(this IQueryable<float> source)
        {
            return await ShardingQueryable<float>.Create(source).FloatAverageAsync();
        }
        public static  float ShardingAverage(this IQueryable<float> source)
        {
            return  ShardingQueryable<float>.Create(source).FloatAverage();
        }
        public static float ShardingAverage<T>(this IQueryable<T> source,Expression<Func<T,float>> keySelector)
        {
            return  ShardingAverage(source.Select(keySelector));
        }

        public static async Task<float> ShardingAverageAsync<T>(this IQueryable<T> source,Expression<Func<T,float>> keySelector)
        {
            return await ShardingAverageAsync(source.Select(keySelector));
        }

    }
}