// using System.Collections.Generic;
// using Microsoft.EntityFrameworkCore;
// using ShardingCore.Extensions;
// using ShardingCore.Sharding.Abstractions;
// using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines;
// using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
// using ShardingCore.Sharding.MergeEngines.Executors.Methods;
//
// namespace ShardingCore.Sharding.StreamMergeEngines
// {
//     /*
//     * @Author: xjm
//     * @Description:
//     * @Date: 2021/8/17 15:16:36
//     * @Ver: 1.0
//     * @Email: 326308290@qq.com
//     */
//     internal class FirstOrDefaultSkipAsyncInMemoryMergeEngine<TEntity> : IEnsureMergeResult<TEntity>
//     {
//         private readonly StreamMergeContext _streamMergeContext;
//
//         public FirstOrDefaultSkipAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext)
//         {
//             _streamMergeContext = streamMergeContext;
//         }
//         // protected override IExecutor<RouteQueryResult<TEntity>> CreateExecutor0(bool async)
//         // {
//         //     return new FirstOrDefaultMethodExecutor<TEntity>(GetStreamMergeContext());
//         // }
//         //
//         // protected override TEntity DoMergeResult0(List<RouteQueryResult<TEntity>> resultList)
//         // {
//         //     var notNullResult = resultList.Where(o => o != null && o.HasQueryResult()).Select(o => o.QueryResult).ToList();
//         //
//         //     if (notNullResult.IsEmpty())
//         //         return default;
//         //
//         //     var streamMergeContext = GetStreamMergeContext();
//         //     if (streamMergeContext.Orders.Any())
//         //         return notNullResult.AsQueryable().OrderWithExpression(streamMergeContext.Orders, streamMergeContext.GetShardingComparer()).FirstOrDefault();
//         //
//         //     return notNullResult.FirstOrDefault();
//         // }
//         public TEntity MergeResult()
//         {
//             return MergeResultAsync().WaitAndUnwrapException(false);
//         }
//
//         public async Task<TEntity> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
//         {
//             
//             //将toke改成1
//             var asyncEnumeratorStreamMergeEngine = new AsyncEnumeratorStreamMergeEngine<TEntity>(_streamMergeContext);
//             
//             var list = new List<TEntity>();
//             await foreach (var element in asyncEnumeratorStreamMergeEngine.WithCancellation(cancellationToken))
//             {
//                 list.Add(element);
//             }
//
//             if (list.IsEmpty())
//             {
//                 return default;
//             }
//             return list.FirstOrDefault();
//         }
//     }
// }
