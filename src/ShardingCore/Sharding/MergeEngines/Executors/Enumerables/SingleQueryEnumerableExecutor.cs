// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using ShardingCore.Sharding.Enumerators;
// using ShardingCore.Sharding.MergeEngines.Abstractions;
// using ShardingCore.Sharding.MergeEngines.Common;
// using ShardingCore.Sharding.MergeEngines.Executors.Enumerables.Abstractions;
//
// namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerables
// {
//     /// <summary>
//     /// 
//     /// </summary>
//     /// Author: xjm
//     /// Created: 2022/5/6 18:31:17
//     /// Email: 326308290@qq.com
//     internal class SingleQueryEnumerableExecutor<TResult> : AbstractEnumerableExecutor<TResult>
//     {
//         public SingleQueryEnumerableExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
//         {
//         }
//         protected override Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync0(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
//         {
//             throw new NotImplementedException();
//         }
//
//         protected override IStreamMergeCombine GetStreamMergeCombine()
//         {
//             throw new NotImplementedException();
//         }
//
//     }
// }
