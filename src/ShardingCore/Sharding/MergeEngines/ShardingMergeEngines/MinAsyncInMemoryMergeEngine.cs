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
    * @Date: 2021/8/18 14:40:06
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class MinAsyncInMemoryMergeEngine<TEntity, TResult> :  AbstractMethodEnsureWrapMergeEngine<TResult>
    {
        public MinAsyncInMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }

        protected override IExecutor<RouteQueryResult<TResult>> CreateExecutor()
        {
            var resultType = typeof(TEntity);
            if (!resultType.IsNullableType())
            {
                if (typeof(decimal) == resultType)
                {
                    return new MinMethodExecutor<TEntity,decimal?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }
                if (typeof(float) == resultType)
                {
                    return new MinMethodExecutor<TEntity, float?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }
                if (typeof(int) == resultType)
                {
                    return new MinMethodExecutor<TEntity, int?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }
                if (typeof(long) == resultType)
                {
                    return new MinMethodExecutor<TEntity, long?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }
                if (typeof(double) == resultType)
                {
                    return new MinMethodExecutor<TEntity, double?>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
                }

                throw new ShardingCoreException($"cant calc min value, type:[{resultType}]");
            }
            else
            {
                return new MinMethodExecutor<TEntity, TEntity>(GetStreamMergeContext()) as IExecutor<RouteQueryResult<TResult>>;
            }
        }
    }
}
