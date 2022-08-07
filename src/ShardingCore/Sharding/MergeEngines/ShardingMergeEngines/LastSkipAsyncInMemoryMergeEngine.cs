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
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:22:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class LastSkipAsyncInMemoryMergeEngine<TEntity> : IEnsureMergeResult<TEntity>
    {
        private readonly StreamMergeContext _streamMergeContext;

        public LastSkipAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext)
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
            var skip = _streamMergeContext.Skip;
            //将toke改成1
            var asyncEnumeratorStreamMergeEngine = new AsyncEnumeratorStreamMergeEngine<TEntity>(_streamMergeContext);

            var maxVirtualElementCount = skip.GetValueOrDefault() + 1;
            var list =  asyncEnumeratorStreamMergeEngine.ToFixedElementStreamList(1,maxVirtualElementCount);

            if (list.VirtualElementCount >= maxVirtualElementCount)
                return list.First();
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        public async Task<TEntity> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var skip = _streamMergeContext.Skip;
            //将toke改成1
            var asyncEnumeratorStreamMergeEngine = new AsyncEnumeratorStreamMergeEngine<TEntity>(_streamMergeContext);
            var maxVirtualElementCount = skip.GetValueOrDefault() + 1;
            var list = await asyncEnumeratorStreamMergeEngine.ToFixedElementStreamListAsync(1,maxVirtualElementCount, cancellationToken);

            if (list.VirtualElementCount >= maxVirtualElementCount)
                return list.First();
            throw new InvalidOperationException("Sequence contains no elements.");
        }
        
        
        // if (notNullResult.IsEmpty())
        // throw new InvalidOperationException("Sequence contains no elements.");
        // var streamMergeContext = GetStreamMergeContext();
        //     if (streamMergeContext.Orders.Any())
        // return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).Last();
        //
        //     return notNullResult.Last();
    }
}

