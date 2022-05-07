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

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 13:37:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AnyAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity,bool>
    {
        public AnyAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override bool DoMergeResult(List<RouteQueryResult<bool>> resultList)
        {
            return resultList.Any(o => o.QueryResult);
        }

        protected override IExecutor<RouteQueryResult<bool>> CreateExecutor0(bool async)
        {
            return new AnyMethodExecutor<TEntity>(GetStreamMergeContext());                
        }
    }
}