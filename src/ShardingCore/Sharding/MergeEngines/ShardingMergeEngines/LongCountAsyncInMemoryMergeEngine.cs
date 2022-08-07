using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Sharding.Abstractions;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 6:34:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class LongCountAsyncInMemoryMergeEngine<TEntity> : AbstractMethodEnsureWrapMergeEngine<long>
    {
        public LongCountAsyncInMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }

        protected override IExecutor<RouteQueryResult<long>> CreateExecutor()
        {
            return new LongCountMethodExecutor<TEntity>(GetStreamMergeContext());
        }
    }
}