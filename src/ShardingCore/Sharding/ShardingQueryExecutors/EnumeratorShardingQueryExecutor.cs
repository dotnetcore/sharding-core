using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync;

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
            if (_streamMergeContext.IsPaginationQuery() && _streamMergeContext.IsSingleShardingTableQuery() && _shardingPageManager.Current != null)
            {
                //获取虚拟表判断是否启用了分页配置
                var shardingEntityType = _streamMergeContext.RouteResults.First().ReplaceTables.First().EntityType;
                var virtualTable = _virtualTableManager.GetVirtualTable(_streamMergeContext.GetShardingDbContext().ShardingDbContextType, shardingEntityType);
                if (virtualTable.EnablePagination)
                {
                    var paginationMetadata = virtualTable.PaginationMetadata;
                    //判断本次查询的排序是否包含order，如果不包含就获取默认添加的排序
                    if (_streamMergeContext.Orders.IsEmpty())
                    {
                        //除了判断属性名还要判断所属关系
                        var appendPaginationConfig = paginationMetadata.PaginationConfigs.OrderByDescending(o => o.AppendOrder)
                            .FirstOrDefault(o => o.AppendIfOrderNone && typeof(TEntity).ContainPropertyName(o.PropertyName) && PaginationMatch(o));
                        if (appendPaginationConfig != null)
                        {
                            return new AppenOrderSequenceEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext, appendPaginationConfig, _shardingPageManager.Current.RouteQueryResults);
                        }
                    }
                    else
                    {
                        var orderCount = _streamMergeContext.Orders.Count();
                        var primaryOrder = _streamMergeContext.Orders.First();
                        if (orderCount == 1)
                        {
                            var sequenceFullMatchOrderConfig = paginationMetadata.PaginationConfigs.Where(o => !o.PaginationMatchEnum.HasFlag(PaginationMatchEnum.PrimaryMatch)).FirstOrDefault(o => PaginationPrimaryMatch(o, primaryOrder));
                            if (sequenceFullMatchOrderConfig != null)
                            {
                                return new SequenceEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext, sequenceFullMatchOrderConfig, _shardingPageManager.Current.RouteQueryResults, primaryOrder.IsAsc);
                            }
                        }

                        var sequencePrimaryMatchOrderConfig = paginationMetadata.PaginationConfigs.Where(o => o.PaginationMatchEnum.HasFlag(PaginationMatchEnum.PrimaryMatch)).FirstOrDefault(o => PaginationPrimaryMatch(o, primaryOrder));
                        if (sequencePrimaryMatchOrderConfig != null)
                        {
                            return new SequenceEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext, sequencePrimaryMatchOrderConfig, _shardingPageManager.Current.RouteQueryResults, primaryOrder.IsAsc);
                        }

                        //skip过大reserve skip
                        if (paginationMetadata.EnableReverseShardingPage && _streamMergeContext.Take.GetValueOrDefault() > 0)
                        {
                            var total = _shardingPageManager.Current.RouteQueryResults.Sum(o => o.QueryResult);
                            if (paginationMetadata.IsUseReverse(_streamMergeContext.Skip.GetValueOrDefault(), total))
                            {
                                return new ReverseShardingEnumeratorAsyncStreamMergeEngine<TEntity>( _streamMergeContext, total);
                            }
                        }

                        //if (paginationMetadata.EnableUnevenShardingPage)
                        //{
                        //    if (paginationMetadata.IsUseUneven(_shardingPageManager.Current.RouteQueryResults, _streamMergeContext.Skip.GetValueOrDefault()))
                        //    {

                        //    }
                        //}
                    }
                }
            }


            return new DefaultShardingEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext);
        }

        private bool PaginationMatch(PaginationSequenceConfig paginationSequenceConfig)
        {
            if (paginationSequenceConfig.PaginationMatchEnum.HasFlag(PaginationMatchEnum.Owner) && !paginationSequenceConfig.PaginationMatchEnum.HasFlag(PaginationMatchEnum.Named))
                return typeof(TEntity) == paginationSequenceConfig.OrderPropertyInfo.DeclaringType;

            return false;
        }

        private bool PaginationPrimaryMatch(PaginationSequenceConfig paginationSequenceConfig, PropertyOrder propertyOrder)
        {
            if (propertyOrder.PropertyExpression != paginationSequenceConfig.PropertyName)
                return false;

            if (paginationSequenceConfig.PaginationMatchEnum.HasFlag(PaginationMatchEnum.Owner) && !paginationSequenceConfig.PaginationMatchEnum.HasFlag(PaginationMatchEnum.Named))
                return typeof(TEntity) == paginationSequenceConfig.OrderPropertyInfo.DeclaringType;

            return false;
        }
    }
}