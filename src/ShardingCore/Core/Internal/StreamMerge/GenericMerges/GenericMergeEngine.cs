using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.StreamMerge.ListMerge;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.StreamMerge.GenericMerges
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 17:04:29
* @Email: 326308290@qq.com
*/
    internal class GenericMergeEngine<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;

        public GenericMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
        }
        
        private async Task<TResult> EFCoreExecute<TResult>(IQueryable<T> newQueryable,RouteResult routeResult,Func<IQueryable, Task<TResult>> efQuery)
        {
            using (var scope = _mergeContext.CreateScope())
            {
                scope.ShardingAccessor.ShardingContext = ShardingContext.Create(routeResult);
                return await efQuery(newQueryable);
            }
        }
        public async Task<List<TResult>> Execute<TResult>(Func<IQueryable, Task<TResult>> efQuery)
        {
            //从各个分表获取数据
            List<DbContext> parallelDbContexts = new List<DbContext>(_mergeContext.RouteResults.Count());
            try
            {
                var enumeratorTasks = _mergeContext.RouteResults.Select(routeResult =>
                {
                    return Task.Run(async () =>
                    {
                        var shardingDbContext = _mergeContext.CreateDbContext();
                        parallelDbContexts.Add(shardingDbContext);
                        var newQueryable = (IQueryable<T>) _mergeContext.GetReWriteQueryable().ReplaceDbContextQueryable(shardingDbContext);
                        
                        return await EFCoreExecute(newQueryable,routeResult,efQuery);
                    });
                }).ToArray();
                return (await Task.WhenAll(enumeratorTasks)).ToList();
            }
            finally
            {
                parallelDbContexts.ForEach(o => o.Dispose());
            }

        }
    }
}