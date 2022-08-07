using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.MergeEngines.AggregateMergeEngines;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers;
using ShardingCore.Sharding.MergeEngines.Executors.Methods.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/7 11:13:57
    /// Email: 326308290@qq.com
    internal class AverageMethodWrapExecutor<TSelect> : AbstractMethodWrapExecutor<AverageResult<TSelect>>
    {
        public AverageMethodWrapExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            var circuitBreaker = new NoTripCircuitBreaker(GetStreamMergeContext());
            circuitBreaker.Register(() =>
            {
                Cancel();
            });
            return circuitBreaker;
        }

        public override IShardingMerger<RouteQueryResult<AverageResult<TSelect>>> GetShardingMerger()
        {
            return new AverageMethodShardingMerger<TSelect>();
        }

        protected override async Task<AverageResult<TSelect>> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken())
        {
            var count = 0L;
            TSelect sum = default;
            var newQueryable = ((IQueryable<TSelect>)queryable);
            var r = await newQueryable.GroupBy(o => 1).BuildExpression().FirstOrDefaultAsync(cancellationToken);
            if (r != null)
            {
                count = r.Item1;
                sum = r.Item2;
            }
            if (count <= 0)
            {
                return default;
            }
            return new AverageResult<TSelect>(sum, count);
        }
    }
}
