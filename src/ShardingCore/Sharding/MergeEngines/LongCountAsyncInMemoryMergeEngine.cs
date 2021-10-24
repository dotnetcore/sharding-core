using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 6:34:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class LongCountAsyncInMemoryMergeEngine<TEntity> : AbstractEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity,long>
    {
        private readonly IShardingPageManager _shardingPageManager;
        public LongCountAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
            _shardingPageManager= ShardingContainer.GetService<IShardingPageManager>();
        }

        public override long MergeResult()
        {
            return AsyncHelper.RunSync(() => MergeResultAsync());
        }

        public override async Task<long> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {

            var result = await base.ExecuteAsync( queryable =>  ((IQueryable<TEntity>)queryable).LongCountAsync(cancellationToken), cancellationToken);

            if (_shardingPageManager.Current != null)
            {
                foreach (var routeQueryResult in result)
                {
                    _shardingPageManager.Current.RouteQueryResults.Add(routeQueryResult);
                }
            }

            return result.Sum(o=>o.QueryResult);
        }
    }
}