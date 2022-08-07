using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;

namespace ShardingCore.Sharding.MergeEngines
{
    
    public class FirstSkipAsyncInMemoryMergeEngine<TEntity> : IEnsureMergeResult<TEntity>
    {
        private readonly StreamMergeContext _streamMergeContext;

        public FirstSkipAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
        }
        // protected override IExecutor<RouteQueryResult<TEntity>> CreateExecutor0(bool async)
        // {
        //     return new FirstOrDefaultMethodExecutor<TEntity>(GetStreamMergeContext());
        // }
        //
        // protected override TEntity DoMergeResult0(List<RouteQueryResult<TEntity>> resultList)
        // {
        //     var notNullResult = resultList.Where(o => o != null && o.HasQueryResult()).Select(o => o.QueryResult).ToList();
        //
        //     if (notNullResult.IsEmpty())
        //         return default;
        //
        //     var streamMergeContext = GetStreamMergeContext();
        //     if (streamMergeContext.Orders.Any())
        //         return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).FirstOrDefault();
        //
        //     return notNullResult.FirstOrDefault();
        // }
        public TEntity MergeResult()
        {
            //将toke改成1
            var asyncEnumeratorStreamMergeEngine = new AsyncEnumeratorStreamMergeEngine<TEntity>(_streamMergeContext);

            var list = asyncEnumeratorStreamMergeEngine.ToStreamList();
            return list.First();
        }

        public async Task<TEntity> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            
            //将toke改成1
            var asyncEnumeratorStreamMergeEngine = new AsyncEnumeratorStreamMergeEngine<TEntity>(_streamMergeContext);
            var take = _streamMergeContext.GetTake();
            var list =await asyncEnumeratorStreamMergeEngine.ToStreamListAsync(take,cancellationToken);
            return list.First();
        }

        // if (notNullResult.IsEmpty())
        // throw new InvalidOperationException("Sequence contains no elements.");
        //
        // var streamMergeContext = GetStreamMergeContext();
        //     if (streamMergeContext.Orders.Any())
        // return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).First();
        //
        //     return notNullResult.First();
    }
}

