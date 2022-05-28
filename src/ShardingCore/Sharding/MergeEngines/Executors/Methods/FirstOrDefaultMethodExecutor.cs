using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.CircuitBreakers;
using ShardingCore.Sharding.MergeEngines.Executors.Methods.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/7 8:48:12
    /// Email: 326308290@qq.com
    internal class FirstOrDefaultMethodExecutor<TEntity> : AbstractOneMethodExecutor<TEntity>
    {
        public FirstOrDefaultMethodExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        public override ICircuitBreaker CreateCircuitBreaker()
        {
            return new AnyElementCircuitBreaker(GetStreamMergeContext());
        }

        protected override Task<OneMethodResult<TEntity>> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken())
        {
            return queryable.As<IQueryable<TEntity>>().Select(o=>new OneMethodResult<TEntity>(o)).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
