using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:30:07
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class ContainsAsyncInMemoryMergeEngine<TEntity>: AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity,bool>
    {
        public ContainsAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }


        protected override bool DoMergeResult(List<RouteQueryResult<bool>> resultList)
        {
            return resultList.Any(o => o.QueryResult);
        }

        protected override IExecutor<RouteQueryResult<bool>> CreateExecutor0(bool async)
        {
            return new ContainsMethodExecutor<TEntity>(GetStreamMergeContext());
        }
    }
}
