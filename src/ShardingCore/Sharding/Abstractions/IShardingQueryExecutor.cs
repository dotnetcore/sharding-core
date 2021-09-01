using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 27 August 2021 22:49:22
* @Email: 326308290@qq.com
*/
    public interface IShardingQueryExecutor
    {
        /// <summary>
        /// 同步执行获取结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="currentContext"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        TResult Execute<TResult>(ICurrentDbContext currentContext, Expression query);
        /// <summary>
        /// 异步执行获取结果
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="currentContext"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        TResult ExecuteAsync<TResult>(ICurrentDbContext currentContext, Expression query, CancellationToken cancellationToken = new CancellationToken());
    }
}