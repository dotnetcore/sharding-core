using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Sharding.Enumerators;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Internal;
#endif

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
        /// execute query
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="currentContext"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        TResult Execute<TResult>(ICurrentDbContext currentContext, Expression query);
        /// <summary>
        /// execute query async
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="currentContext"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        TResult ExecuteAsync<TResult>(ICurrentDbContext currentContext, Expression query, CancellationToken cancellationToken = new CancellationToken());
    }
}