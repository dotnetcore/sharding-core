using ShardingCore.Sharding.StreamMergeEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Helpers;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 14:22:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractInMemoryAsyncMergeEngine<TEntity> : AbstractBaseMergeEngine<TEntity>, IInMemoryAsyncMergeEngine
    {
        private readonly StreamMergeContext _mergeContext;

        protected AbstractInMemoryAsyncMergeEngine(StreamMergeContext streamMergeContext)
        {
            _mergeContext = streamMergeContext;
        }

        public async Task<List<RouteQueryResult<TResult>>> ExecuteAsync<TResult>(CancellationToken cancellationToken = new CancellationToken())
        {
            var routeQueryResults = _mergeContext.PreperExecute(() => new List<RouteQueryResult<TResult>>(0));
            if (routeQueryResults != null)
                return routeQueryResults;
            var defaultSqlRouteUnits = GetDefaultSqlRouteUnits();
            var waitExecuteQueue = GetDataSourceGroupAndExecutorGroup<RouteQueryResult<TResult>>(true, defaultSqlRouteUnits, cancellationToken).ToArray();

            return (await TaskHelper.WhenAllFastFail(waitExecuteQueue)).SelectMany(o => o).ToList();
        }


        protected override StreamMergeContext GetStreamMergeContext()
        {
            return _mergeContext;
        }
    }
}