using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 14:22:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractInMemoryAsyncMergeEngine<T>
    {
        /// <summary>
        /// 获取流失合并上下文
        /// </summary>
        /// <returns></returns>
        protected abstract StreamMergeContext<T> GetStreamMergeContext();

        public async Task<List<TResult>> ExecuteAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery,CancellationToken cancellationToken = new CancellationToken())
        {
            var tableResult = GetStreamMergeContext().GetRouteResults();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                if (routeResult.ReplaceTables.Count > 1)
                    throw new ShardingCoreException("route found more than 1 table name s");
                var tail = string.Empty;
                if (routeResult.ReplaceTables.Count == 1)
                    tail = routeResult.ReplaceTables.First().Tail;

                return Task.Run(async () =>
                {
                    try
                    {
                        //using (var scope = _mergeContext.CreateScope())
                        //{
                        //var shardingContext = ShardingContext.Create(routeResult);
                        //scope.ShardingAccessor.ShardingContext = shardingContext;

                        var shardingDbContext = GetStreamMergeContext().CreateDbContext(tail);
                        var newQueryable = (IQueryable<T>)GetStreamMergeContext().GetReWriteQueryable()
                                .ReplaceDbContextQueryable(shardingDbContext);
                        var newFilterQueryable=EFQueryAfterFilter(newQueryable);
                        var query = await efQuery(newFilterQueryable);
                        return query;
                        //}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();

            return  (await Task.WhenAll(enumeratorTasks)).ToList();
        }

        public virtual IQueryable EFQueryAfterFilter(IQueryable<T> queryable)
        {
            return queryable;
        }

    }
}
