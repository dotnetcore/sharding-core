using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines
{
    internal class ReverseStreamMergeCombine:IStreamMergeCombine
    {
        private static readonly IStreamMergeCombine _instance;
        static ReverseStreamMergeCombine()
        {
            _instance = new ReverseStreamMergeCombine();
        }

        private ReverseStreamMergeCombine()
        {
            
        }

        public static IStreamMergeCombine Instance => _instance;
        public IStreamMergeAsyncEnumerator<TEntity> StreamMergeEnumeratorCombine<TEntity>(StreamMergeContext streamMergeContext,
            IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            var doGetStreamMergeAsyncEnumerator = DoGetStreamMergeAsyncEnumerator(streamMergeContext, streamsAsyncEnumerators);
            return new InMemoryReverseStreamMergeAsyncEnumerator<TEntity>(doGetStreamMergeAsyncEnumerator);
        }

        private static IStreamMergeAsyncEnumerator<TEntity> DoGetStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext streamMergeContext, IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (streamMergeContext.IsPaginationQuery())
            {
                if (streamMergeContext.HasGroupQuery())
                {
                    var multiAggregateOrderStreamMergeAsyncEnumerator = new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(streamMergeContext, streamsAsyncEnumerators);
                    return new PaginationStreamMergeAsyncEnumerator<TEntity>(streamMergeContext, new[] { multiAggregateOrderStreamMergeAsyncEnumerator });
                }
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(streamMergeContext, streamsAsyncEnumerators);
            }
            if (streamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(streamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(streamMergeContext, streamsAsyncEnumerators);
        }
    }
}
