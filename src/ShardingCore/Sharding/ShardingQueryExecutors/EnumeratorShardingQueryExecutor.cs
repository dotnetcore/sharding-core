using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions;

namespace ShardingCore.Sharding.ShardingQueryExecutors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/31 21:30:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class EnumeratorShardingQueryExecutor<TEntity>
    {
        private readonly StreamMergeContext<TEntity> _streamMergeContext;
        private readonly IShardingPageManager _shardingPageManager;

        public EnumeratorShardingQueryExecutor(StreamMergeContext<TEntity> streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
            _shardingPageManager = ShardingContainer.GetService<IShardingPageManager>();
        }
        public IEnumeratorStreamMergeEngine<TEntity> ExecuteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            //操作单表
            if (!_streamMergeContext.IsShardingQuery())
            {
                return new SingleQueryEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext);
            }
            //未开启系统分表
            if (_shardingPageManager.Current == null)
            {
                return new DefaultShardingEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext);
            }
            
            
        }
        
    }
}
