using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;
using ShardingCore.Sharding.MergeEngines.ParallelControls.Enumerators;
using ShardingCore.Sharding.MergeEngines.ParallelExecutors;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 02 September 2021 20:58:10
    * @Email: 326308290@qq.com
    */
    internal class SingleQueryEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity> : AbstractEnumeratorStreamMergeEngine<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public SingleQueryEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext, new SingleStreamMergeCombine<TEntity>())
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataSourceName = StreamMergeContext.DataSourceRouteResult.IntersectDataSources.First();
            var routeResult = StreamMergeContext.TableRouteResults[0];
            var shardingDbContext = StreamMergeContext.CreateDbContext(dataSourceName, routeResult, ConnectionModeEnum.MEMORY_STRICTLY);
            var newQueryable = (IQueryable<TEntity>)StreamMergeContext.GetOriginalQueryable().ReplaceDbContextQueryable(shardingDbContext);
            var enumeratorParallelExecutor = new SingleQueryEnumeratorParallelExecutor<TEntity>();
            if (async)
            {
                var asyncEnumerator = enumeratorParallelExecutor.GetAsyncEnumerator0(newQueryable).WaitAndUnwrapException();
                return new[] { new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator) };
            }
            else
            {
                var enumerator = enumeratorParallelExecutor.GetEnumerator0(newQueryable);
                return new[] { new StreamMergeAsyncEnumerator<TEntity>(enumerator) };
            }
        }

        protected override IParallelExecuteControl<IStreamMergeAsyncEnumerator<TEntity>> CreateParallelExecuteControl0(IParallelExecutor<IStreamMergeAsyncEnumerator<TEntity>> executor)
        {
            return new SingleQueryEnumeratorParallelExecuteControl<TEntity>(GetStreamMergeContext(), executor, GetStreamMergeCombine());
        }
    }
}