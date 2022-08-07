// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using ShardingCore.Sharding.Enumerators;
// using ShardingCore.Sharding.MergeEngines.Abstractions;
// using ShardingCore.Sharding.MergeEngines.Common;
// using ShardingCore.Sharding.MergeEngines.Executors.Enumerables.Abstractions;
// using ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers;
//
// namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerables
// {
//     /// <summary>
//     /// 
//     /// </summary>
//     /// Author: xjm
//     /// Created: 2022/5/6 13:10:34
//     /// Email: 326308290@qq.com
//     internal class EmptyShardingExecutor<TResult> : AbstractEnumerableExecutor<TResult>
//     {
//         // private readonly IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> _shardingMerger;
//
//         public EmptyShardingExecutor(StreamMergeContext streamMergeContext, bool async) : base(streamMergeContext)
//         {
//             // _shardingMerger = new EmptyEnumerableShardingMerger<TResult>(streamMergeContext,async);
//         }
//
//         protected override Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync0(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
//         {
//             throw new NotImplementedException();
//         }
//
//
//         public override IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> GetShardingMerger()
//         {
//             throw new NotImplementedException();
//         }
//     }
// }
