using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.PaginationConfigurations;
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
        private readonly IVirtualTableManager _virtualTableManager;

        public EnumeratorShardingQueryExecutor(StreamMergeContext<TEntity> streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
            _shardingPageManager = ShardingContainer.GetService<IShardingPageManager>();
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager>();
        }
        public IEnumeratorStreamMergeEngine<TEntity> ExecuteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            //操作单表
            if (!_streamMergeContext.IsShardingQuery())
            {
                return new SingleQueryEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext);
            }
            //未开启系统分表或者本次查询涉及多张分表
            if (!_streamMergeContext.IsPaginationQuery()||!_streamMergeContext.IsSingleShardingTableQuery()||_shardingPageManager.Current == null)
            {
                return new DefaultShardingEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext);
            }
            var shardingEntityType = _streamMergeContext.RouteResults.First().ReplaceTables.Single(o=>o.IsShardingTable()).EntityType;
            var virtualTable = _virtualTableManager.GetVirtualTable(_streamMergeContext.GetShardingDbContext().ShardingDbContextType,shardingEntityType);
            if (!virtualTable.EnablePagination)
                return new DefaultShardingEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext);
            if (_streamMergeContext.Orders.IsEmpty())
            {
                var append = virtualTable.PaginationMetadata.PaginationConfigs.FirstOrDefault(o=>o.AppendIfOrderNone);
                if (append != null)
                {
                    123
                    return new SequenceEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext);
                }
            }

            var propertyOrder = _streamMergeContext.Orders.First();
            //PaginationMatchEnum.Owner
            111
            var paginationConfig = virtualTable.PaginationMetadata.PaginationConfigs.FirstOrDefault(o=>o.PropertyName==propertyOrder.PropertyExpression);
            if (paginationConfig==null)
                return new DefaultShardingEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext);
            //调用顺序排序
            paginationConfig
            
        }

    }
}
