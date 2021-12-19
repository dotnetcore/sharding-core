using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.ShardingExecutors;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

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
        /// <param name="mergeQueryCompilerContext"></param>
        /// <returns></returns>
        TResult Execute<TResult>(IMergeQueryCompilerContext mergeQueryCompilerContext);
        /// <summary>
        /// execute query async
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="mergeQueryCompilerContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        TResult ExecuteAsync<TResult>(IMergeQueryCompilerContext mergeQueryCompilerContext, CancellationToken cancellationToken = new CancellationToken());
    }
}