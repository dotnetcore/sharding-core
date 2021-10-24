using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/24 10:16:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract  class AbstractTrackGenericMethodCallWhereInMemoryAsyncMergeEngine<TShardingDbContext,TEntity> : AbstractGenericMethodCallWhereInMemoryAsyncMergeEngine<TEntity> where TShardingDbContext:DbContext,IShardingDbContext
    {
        protected AbstractTrackGenericMethodCallWhereInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }
        public override TResult MergeResult<TResult>()
        {
            var current = DoMergeResult<TResult>();
            if (current != null)
            {
                if (GetStreamMergeContext().IsUseShardingTrack(current.GetType()))
                {
                    var c = (object)current;
                    var genericDbContext = GetStreamMergeContext().GetShardingDbContext().CreateGenericDbContext(c);
                    var attachedEntity = genericDbContext.GetAttachedEntity(c);
                    if (attachedEntity == null)
                        genericDbContext.Attach(current);
                    else
                    {
                        return (TResult)attachedEntity;
                    }
                }

            }
            return current;
        }

        public override async Task<TResult> MergeResultAsync<TResult>(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = await DoMergeResultAsync<TResult>(cancellationToken);
            if (current != null)
            {
                if (GetStreamMergeContext().IsUseShardingTrack(current.GetType()))
                {
                    var c = (object)current;
                    var genericDbContext = GetStreamMergeContext().GetShardingDbContext().CreateGenericDbContext(c);
                    var attachedEntity = genericDbContext.GetAttachedEntity(c);
                    if (attachedEntity == null)
                        genericDbContext.Attach(current);
                    else
                    {
                        return (TResult)attachedEntity;
                    }
                }
            }
            return current;
        }
        public abstract TResult DoMergeResult<TResult>();

        public abstract Task<TResult> DoMergeResultAsync<TResult>(
            CancellationToken cancellationToken = new CancellationToken());


    }
}
