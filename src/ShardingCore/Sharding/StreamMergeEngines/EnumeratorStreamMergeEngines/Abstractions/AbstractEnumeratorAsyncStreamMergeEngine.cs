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
    public abstract class AbstractEnumeratorAsyncStreamMergeEngine<TEntity> : AbstractEnumeratorStreamMergeEngine<TEntity>
    {
        public AbstractEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }


        public abstract IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators(bool async);
        public abstract IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators);

        public Task<StreamMergeAsyncEnumerator<TEntity>> AsyncQueryEnumerator(IQueryable<TEntity> queryable,bool async)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (async)
                    {
                        var asyncEnumerator = await DoGetAsyncEnumerator(queryable);
                        return new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator);
                    }
                    else
                    {
                        var enumerator =  DoGetEnumerator(queryable);
                        return new StreamMergeAsyncEnumerator<TEntity>(enumerator);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });
        }
        public async Task<IAsyncEnumerator<TEntity>> DoGetAsyncEnumerator(IQueryable<TEntity> newQueryable)
        {
            var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
            await enumator.MoveNextAsync();
            return enumator;
        }
        public IEnumerator<TEntity> DoGetEnumerator(IQueryable<TEntity> newQueryable)
        {
            var enumator = newQueryable.AsEnumerable().GetEnumerator();
             enumator.MoveNext();
            return enumator;
        }
        // public virtual IQueryable<TEntity> CreateAsyncExecuteQueryable(TableRouteResult tableRouteResult)
        // {
        //     var shardingDbContext = StreamMergeContext.CreateDbContext(tableRouteResult);
        //     var useOriginal = StreamMergeContext > 1;
        //     DbContextQueryStore.TryAdd(tableRouteResult,shardingDbContext);
        //     var newQueryable = (IQueryable<TEntity>)(useOriginal ? StreamMergeContext.GetReWriteQueryable() : StreamMergeContext.GetOriginalQueryable())
        //         .ReplaceDbContextQueryable(shardingDbContext);
        //     return newQueryable;
        // }
        
        public override IStreamMergeAsyncEnumerator<TEntity> GetShardingAsyncEnumerator(bool async,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var dbStreamMergeAsyncEnumerators = GetDbStreamMergeAsyncEnumerators(async);
            if (dbStreamMergeAsyncEnumerators.IsEmpty())
                throw new ShardingCoreException("GetDbStreamMergeAsyncEnumerators empty");
            return GetStreamMergeAsyncEnumerator(dbStreamMergeAsyncEnumerators);
        }
    }
}
