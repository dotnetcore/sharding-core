using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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
        public LongCountAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
            _shardingPageManager= ShardingContainer.GetService<IShardingPageManager>();
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