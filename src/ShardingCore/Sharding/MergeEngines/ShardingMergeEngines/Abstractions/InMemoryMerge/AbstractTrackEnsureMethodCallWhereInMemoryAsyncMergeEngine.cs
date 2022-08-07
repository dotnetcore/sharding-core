// using System.Collections.Generic;
// using System.Linq.Expressions;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using ShardingCore.Extensions;
// using ShardingCore.Sharding.Abstractions;
// using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
// using ShardingCore.Sharding.StreamMergeEngines;
//
// namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge
// {
//     /*
//     * @Author: xjm
//     * @Description:
//     * @Date: 2021/9/24 10:16:28
//     * @Ver: 1.0
//     * @Email: 326308290@qq.com
//     */
//     internal abstract  class AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity> : AbstractMethodEnsureMergeEngine<TEntity,TEntity>
//     {
//         protected AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine(StreamMergeContext streamStreamMergeContext,IExecutor executor) : base(streamStreamMergeContext,executor)
//         {
//         }
//
//         private TEntity ProcessTrackResult(TEntity current)
//         {
//             if (current != null)
//             {
//                 if (GetStreamMergeContext().IsUseShardingTrack(current.GetType()))
//                 {
//                     var c = (object)current;
//                     var genericDbContext = GetStreamMergeContext().GetShardingDbContext().CreateGenericDbContext(c);
//                     var attachedEntity = genericDbContext.GetAttachedEntity(c);
//                     if (attachedEntity == null)
//                         genericDbContext.Attach(current);
//                     else
//                     {
//                         return (TEntity)attachedEntity;
//                     }
//                 }
//
//             }
//             return current;
//         }
//
//         protected override TEntity DoMergeResult(List<RouteQueryResult<TEntity>> resultList)
//         {
//             var entity = DoMergeResult0(resultList);
//             return ProcessTrackResult(entity);
//         }
//
//         protected abstract TEntity DoMergeResult0(List<RouteQueryResult<TEntity>> resultList);
//     }
// }
