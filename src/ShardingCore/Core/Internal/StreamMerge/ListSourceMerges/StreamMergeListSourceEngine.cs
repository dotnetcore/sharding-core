using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.StreamMerge.ListMerge;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Extensions;
#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Core.Internal.StreamMerge.ListSourceMerges
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 14:45:19
* @Email: 326308290@qq.com
*/
    internal class StreamMergeListSourceEngine<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;

        public StreamMergeListSourceEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
        }
        
        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator(IQueryable<T> newQueryable,RouteResult routeResult)
        {
            using (var scope = _mergeContext.CreateScope())
            {
                scope.ShardingAccessor.ShardingContext = ShardingContext.Create(routeResult);
#if !EFCORE2
                var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
                await enumator.MoveNextAsync();
#endif
#if EFCORE2

                var enumator = newQueryable.AsAsyncEnumerable().GetEnumerator();
                await enumator.MoveNext();
#endif
                return enumator;
            }
        }
        public async Task<List<T>> Execute()
        {
            //去除分页,获取前Take+Skip数量
            var noPageSource = _mergeContext.Source.RemoveTake().RemoveSkip();
            if (_mergeContext.Take.HasValue)
                noPageSource = noPageSource.Take(_mergeContext.Take.Value + _mergeContext.Skip.GetValueOrDefault());
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
                        var newQueryable = (IQueryable<T>) noPageSource.ReplaceDbContextQueryable(shardingDbContext);
                        
#if !EFCORE2
                        return await GetAsyncEnumerator(newQueryable,routeResult);
#endif
#if EFCORE2

                        return await GetAsyncEnumerator(newQueryable,routeResult);
#endif
                    });
                }).ToArray();
                var enumerators = (await Task.WhenAll(enumeratorTasks)).ToList();

                var engine = new StreamMergeListEngine<T>(_mergeContext, enumerators);

                return await engine.Execute();
            }
            finally
            {
                parallelDbContexts.ForEach(o => o.Dispose());
            }

        }
    }
}