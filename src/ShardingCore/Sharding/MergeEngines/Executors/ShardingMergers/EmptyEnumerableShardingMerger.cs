// using System.Collections.Generic;
// using ShardingCore.Exceptions;
// using ShardingCore.Sharding.Enumerators;
//
// namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
// {
//     internal class EmptyEnumerableShardingMerger<TEntity> : AbstractEnumerableShardingMerger<TEntity>
//     {
//         public EmptyEnumerableShardingMerger(StreamMergeContext streamMergeContext, bool async) : base(
//             streamMergeContext, async)
//         {
//         }
//
//         public override IStreamMergeAsyncEnumerator<TEntity> StreamMerge(
//             List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
//         {
//             if (parallelResults.Count != 1)
//                 throw new ShardingCoreInvalidOperationException(
//                     $"empty query combine has more {nameof(IStreamMergeAsyncEnumerator<TEntity>)}");
//             return parallelResults[0];
//         }
//
//         public override void InMemoryMerge(List<IStreamMergeAsyncEnumerator<TEntity>> beforeInMemoryResults, List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
//         {
//             var previewResultsCount = beforeInMemoryResults.Count;
//             if (previewResultsCount > 1)
//             {
//                 throw new ShardingCoreInvalidOperationException(
//                     $"{typeof(TEntity)} {nameof(beforeInMemoryResults)} has more than one element in container");
//             }
//
//             var parallelCount = parallelResults.Count;
//             if (parallelCount == 0)
//                 return;
//             beforeInMemoryResults.Clear();
//             beforeInMemoryResults.Add(parallelResults[0]);
//         }
//     }
// }