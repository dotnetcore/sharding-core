using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:26:40
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class SingleOrDefaultAsyncInMemoryMergeEngine<TEntity> : AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity>
    {
        public SingleOrDefaultAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }
        protected override IExecutor<RouteQueryResult<TEntity>> CreateExecutor0(bool async)
        {
            return new SingleOrDefaultMethodExecutor<TEntity>(GetStreamMergeContext());
        }

        protected override TEntity DoMergeResult0(List<RouteQueryResult<TEntity>> resultList)
        {
            var notNullResult = resultList.Where(o => o != null && o.HasQueryResult()).Select(o => o.QueryResult).ToList();

            if (notNullResult.Count > 1)
                throw new InvalidOperationException("Sequence contains more than one element.");
            return notNullResult.SingleOrDefault();
        }
    }
}
