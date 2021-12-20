using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/24 10:16:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract  class AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine<TEntity> : AbstractEnsureMethodCallInMemoryAsyncMergeEngine<TEntity,TEntity>
    {
        protected AbstractTrackEnsureMethodCallWhereInMemoryAsyncMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        private TEntity ProcessTrackResult(TEntity current)
        {
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
                        return (TEntity)attachedEntity;
                    }
                }

            }
            return current;
        }
        public override async Task<TEntity> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = await DoMergeResultAsync(cancellationToken);
            return ProcessTrackResult(current);
        }

        public abstract Task<TEntity> DoMergeResultAsync(
            CancellationToken cancellationToken = new CancellationToken());


    }
}
