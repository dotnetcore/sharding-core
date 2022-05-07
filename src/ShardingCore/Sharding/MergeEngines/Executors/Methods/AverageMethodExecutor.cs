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

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/7 11:13:57
    /// Email: 326308290@qq.com
    internal class AverageMethodExecutor<TEntity> : AbstractMethodExecutor<AverageResult<TEntity>>
    {
        public AverageMethodExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            return new NoTripCircuitBreaker(GetStreamMergeContext());
        }

        protected override async Task<AverageResult<TEntity>> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken())
        {
            var count = 0L;
            TEntity sum = default;
            var newQueryable = ((IQueryable<TEntity>)queryable);
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
            return new AverageResult<TEntity>(sum, count);
        }
    }
}
