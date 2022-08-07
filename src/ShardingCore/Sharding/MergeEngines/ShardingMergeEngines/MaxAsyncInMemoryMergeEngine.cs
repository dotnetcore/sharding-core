using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:44:53
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class MaxAsyncInMemoryMergeEngine<TEntity, TResult> : AbstractMethodEnsureWrapMergeEngine<TResult>
    {
        public MaxAsyncInMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }

        protected override IExecutor<RouteQueryResult<TResult>> CreateExecutor()
        {
            var resultType = typeof(TEntity);
            if (!resultType.IsNullableType())
            {
                if (typeof(decimal) == resultType)
                {
                    return new MaxMethodExecutor<TEntity,decimal?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }
                if (typeof(float) == resultType)
                {
                    return new MaxMethodExecutor<TEntity, float?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }
                if (typeof(int) == resultType)
                {
                    return new MaxMethodExecutor<TEntity, int?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }
                if (typeof(long) == resultType)
                {
                    return new MaxMethodExecutor<TEntity, long?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }
                if (typeof(double) == resultType)
                {
                    return new MaxMethodExecutor<TEntity, double?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }

                throw new ShardingCoreException($"cant calc max value, type:[{resultType}]");
            }
            else
            {
                return new MaxMethodExecutor<TEntity,TEntity>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
            }
        }
    }
}
