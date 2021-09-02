using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 15:35:39
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractEnumeratorStreamMergeEngine<TEntity>:IEnumeratorStreamMergeEngine<TEntity>
    {
        public StreamMergeContext<TEntity> StreamMergeContext { get; }
        public ConcurrentDictionary<RouteResult,DbContext> DbContextQueryStore { get; }

        public AbstractEnumeratorStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext)
        {
            StreamMergeContext = streamMergeContext;
            DbContextQueryStore = new ConcurrentDictionary<RouteResult, DbContext>();
        }

        public abstract IAsyncEnumerator<TEntity> GetAsyncEnumerator(
            CancellationToken cancellationToken = new CancellationToken());

        public abstract IEnumerator<TEntity> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (DbContextQueryStore.IsNotEmpty())
            {
                DbContextQueryStore.Values.ForEach(dbContext =>
                {
                    dbContext.Dispose();
                });
            }
        }
    }
}
