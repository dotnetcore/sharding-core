using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualTables;

namespace ShardingCore.Core
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 22 December 2020 16:29:18
* @Email: 326308290@qq.com
*/
    public interface IShardingQueryable<T>
    {
        /// <summary>
        /// 启用自动路由解析
        /// </summary>
        /// <returns></returns>
        IShardingQueryable<T> EnableAutoRouteParse();
        /// <summary>
        /// 禁用自动路由解析
        /// </summary>
        /// <returns></returns>
        IShardingQueryable<T> DisableAutoRouteParse();
        /// <summary>
        /// 设置其他表条件
        /// </summary>
        /// <param name="predicate"></param>
        /// <typeparam name="TShardingEntity"></typeparam>
        /// <returns></returns>
        IShardingQueryable<T> AddManualRoute<TShardingEntity>(Expression<Func<TShardingEntity, bool>> predicate) where TShardingEntity : class, IShardingEntity;
        /// <summary>
        /// 手动指定路由
        /// </summary>
        /// <param name="tail"></param>
        /// <returns></returns>
        IShardingQueryable<T> AddManualRoute<TShardingEntity>(string tail) where TShardingEntity : class, IShardingEntity;
        
        /// <summary>
        /// 异步获取数量
        /// </summary>
        /// <returns></returns>
        Task<int> CountAsync();
        /// <summary>
        /// 异步获取数量
        /// </summary>
        /// <returns></returns>
        Task<long> LongCountAsync();


        /// <summary>
        /// 异步获取列表
        /// </summary>
        /// <returns></returns>
        Task<List<T>> ToListAsync(int capacity=20);


        /// <summary>
        /// 获取第一个,若不存在则返回默认值
        /// </summary>
        /// <returns></returns>
        Task<T> FirstOrDefaultAsync();


        /// <summary>
        /// 异步判断是否存在
        /// </summary>
        /// <param name="predicate">表达式</param>
        /// <returns></returns>
        Task<bool> AnyAsync();


        /// <summary>
        /// 异步计算最大值
        /// </summary>
        /// <returns></returns>
        Task<T> MaxAsync();


        /// <summary>
        /// 异步计算最小值
        /// </summary>
        /// <returns></returns>
        Task<T> MinAsync();

        /// <summary>
        /// 异步求和
        /// </summary>
        /// <returns></returns>
        Task<int> SumAsync();
        /// <summary>
        /// 异步求和
        /// </summary>
        /// <returns></returns>
        Task<long> LongSumAsync();
        /// <summary>
        /// 异步求和
        /// </summary>
        /// <returns></returns>
        Task<decimal> DecimalSumAsync();
        /// <summary>
        /// 异步求和
        /// </summary>
        /// <returns></returns>
        Task<double> DoubleSumAsync();
        /// <summary>
        /// 异步求和
        /// </summary>
        /// <returns></returns>
        Task<float> FloatSumAsync();

        /// <summary>
        /// 平均数
        /// </summary>
        /// <returns></returns>
        Task<double> AverageAsync();
        /// <summary>
        /// 平均数
        /// </summary>
        /// <returns></returns>
        Task<double> LongAverageAsync();
        /// <summary>
        /// 平均数
        /// </summary>
        /// <returns></returns>
        Task<decimal> DecimalAverageAsync();
        /// <summary>
        /// 平均数
        /// </summary>
        /// <returns></returns>
        Task<double> DoubleAverageAsync();
        /// <summary>
        /// 平均数
        /// </summary>
        /// <returns></returns>
        Task<float> FloatAverageAsync();

    }
}