using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
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
    * @Date: 2021/8/18 6:34:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class LongCountAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity,long>
    {
        private readonly IShardingPageManager _shardingPageManager;
        public LongCountAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
            _shardingPageManager= ShardingContainer.GetService<IShardingPageManager>();
        }

        protected override long DoMergeResult(List<RouteQueryResult<long>> resultList)
        {

            if (_shardingPageManager.Current != null)
            {
                long r = 0;
                foreach (var routeQueryResult in resultList)
                {
                    _shardingPageManager.Current.RouteQueryResults.Add(routeQueryResult);
                    r+= routeQueryResult.QueryResult;
                }

                return r;
            }

            return resultList.Sum(o => o.QueryResult);
        }

        protected override IExecutor<RouteQueryResult<long>> CreateExecutor0(bool async)
        {
            return new LongCountMethodExecutor<TEntity>(GetStreamMergeContext());
        }
    }
}