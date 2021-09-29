using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions.AbstractGenericExpressionMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/24 10:16:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract  class AbstractTrackGenericMethodCallWhereInMemoryAsyncMergeEngine<TShardingDbContext,TEntity> : AbstractGenericMethodCallWhereInMemoryAsyncMergeEngine<TEntity> where TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly ITrackerManager<TShardingDbContext> _trackerManager;
        protected AbstractTrackGenericMethodCallWhereInMemoryAsyncMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
            _trackerManager = ShardingContainer.GetService<ITrackerManager<TShardingDbContext>>();
        }
        /// <summary>
        /// 手动追踪
        /// </summary>
        private bool IsUseManualTrack => GetIsUseManualTrack();

        private bool GetIsUseManualTrack()
        {
            if (!GetStreamMergeContext().IsCrossTable)
                return false;
            if (GetStreamMergeContext().IsNoTracking.HasValue)
            {
                return !GetStreamMergeContext().IsNoTracking.Value;
            }
            else
            {
                return ((DbContext)GetStreamMergeContext().GetShardingDbContext()).ChangeTracker.QueryTrackingBehavior ==
                       QueryTrackingBehavior.TrackAll;
            }
        }
        public override TResult MergeResult<TResult>()
        {
            var current = DoMergeResult<TResult>();
            if (current != null)
            {
                if (IsUseManualTrack && _trackerManager.EntityUseTrack(current.GetType()))
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
                if (IsUseManualTrack && _trackerManager.EntityUseTrack(current.GetType()))
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
