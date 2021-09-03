using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 15:38:05
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractEnumeratorAsyncStreamMergeEngine<TEntity>: AbstractEnumeratorStreamMergeEngine<TEntity>
    {
        public AbstractEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            var dbStreamMergeAsyncEnumerators = GetDbStreamMergeAsyncEnumerators();
            if (dbStreamMergeAsyncEnumerators.IsEmpty())
                throw new ShardingCoreException("GetDbStreamMergeAsyncEnumerators empty");
            return GetStreamMergeAsyncEnumerator(dbStreamMergeAsyncEnumerators);
        }

        public abstract IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators();
        public abstract IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators);
        public async Task<IAsyncEnumerator<TEntity>> DoGetAsyncEnumerator(IQueryable<TEntity> newQueryable)
        {
            var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
            await enumator.MoveNextAsync();
            return enumator;
        }
        // public virtual IQueryable<TEntity> CreateAsyncExecuteQueryable(RouteResult routeResult)
        // {
        //     var shardingDbContext = StreamMergeContext.CreateDbContext(routeResult);
        //     var useOriginal = StreamMergeContext > 1;
        //     DbContextQueryStore.TryAdd(routeResult,shardingDbContext);
        //     var newQueryable = (IQueryable<TEntity>)(useOriginal ? StreamMergeContext.GetReWriteQueryable() : StreamMergeContext.GetOriginalQueryable())
        //         .ReplaceDbContextQueryable(shardingDbContext);
        //     return newQueryable;
        // }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
