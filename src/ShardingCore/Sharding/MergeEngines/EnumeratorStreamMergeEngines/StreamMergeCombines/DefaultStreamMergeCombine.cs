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
    internal class DefaultStreamMergeCombine:IStreamMergeCombine
    {

        private static readonly IStreamMergeCombine _instance;
        static DefaultStreamMergeCombine()
        {
            _instance = new DefaultStreamMergeCombine();
        }

        private DefaultStreamMergeCombine()
        {

        }
        public static IStreamMergeCombine Instance => _instance;
        public IStreamMergeAsyncEnumerator<TEntity> StreamMergeEnumeratorCombine<TEntity>(StreamMergeContext streamMergeContext,
            IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (streamMergeContext.IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(streamMergeContext, streamsAsyncEnumerators);
            if (streamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(streamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(streamMergeContext, streamsAsyncEnumerators);
        }
    }
}
