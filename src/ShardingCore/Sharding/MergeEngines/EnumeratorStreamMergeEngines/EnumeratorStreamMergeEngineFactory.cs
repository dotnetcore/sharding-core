using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.Enumerables;
using ShardingCore.Sharding.PaginationConfigurations;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/31 21:30:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class EnumeratorStreamMergeEngineFactory<TShardingDbContext, TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly StreamMergeContext _streamMergeContext;
        private readonly IShardingPageManager _shardingPageManager;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private EnumeratorStreamMergeEngineFactory(StreamMergeContext streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
            _shardingPageManager = ShardingContainer.GetService<IShardingPageManager>();
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
            _entityMetadataManager = ShardingContainer.GetService<IEntityMetadataManager<TShardingDbContext>>();
        }

        public static EnumeratorStreamMergeEngineFactory<TShardingDbContext, TEntity> Create(StreamMergeContext streamMergeContext)
        {
            return new EnumeratorStreamMergeEngineFactory<TShardingDbContext, TEntity>(streamMergeContext);
        }

        public IVirtualDataSourceRoute GetRoute(Type entityType)
        {
            return _streamMergeContext.GetShardingDbContext().GetVirtualDataSource().GetRoute(entityType);
        }
        public IStreamEnumerable<TEntity> GetStreamEnumerable()
        {
            if (_streamMergeContext.IsRouteNotMatch())
            {
                return new EmptyQueryStreamEnumerable<TShardingDbContext, TEntity>(_streamMergeContext);
            }
            //本次查询没有跨库没有跨表就可以直接执行
            if (!_streamMergeContext.IsMergeQuery())
            {
                return new SingleQueryStreamEnumerable<TShardingDbContext, TEntity>(_streamMergeContext);
            }

            if (_streamMergeContext.UseUnionAllMerge())
            {
                return new DefaultShardingStreamEnumerable<TShardingDbContext, TEntity>(_streamMergeContext);
            }

            //未开启系统分表或者本次查询涉及多张分表
            if (_streamMergeContext.IsPaginationQuery() && _streamMergeContext.IsSingleShardingEntityQuery() && _shardingPageManager.Current != null)
            {
                //获取虚拟表判断是否启用了分页配置
                var shardingEntityType = _streamMergeContext.GetSingleShardingEntityType();
                if (shardingEntityType == null)
                    throw new ShardingCoreException($"query not found sharding data source or sharding table entity");

                if (_streamMergeContext.Orders.IsEmpty())
                {
                    //自动添加属性顺序排序
                    //除了判断属性名还要判断所属关系
                    var mergeEngine = DoNoOrderAppendEnumeratorStreamMergeEngine(shardingEntityType);
                    if (mergeEngine != null)
                        return mergeEngine;
                }
                else
                {
                    var mergeEngine = DoOrderSequencePaginationEnumeratorStreamMergeEngine(shardingEntityType);

                    if (mergeEngine != null)
                        return mergeEngine;
                }
            }


            return new DefaultShardingStreamEnumerable<TShardingDbContext, TEntity>(_streamMergeContext);
        }

        private IStreamEnumerable<TEntity> DoNoOrderAppendEnumeratorStreamMergeEngine(Type shardingEntityType)
        {

            var isShardingDataSource = _entityMetadataManager.IsShardingDataSource(shardingEntityType);
            var isShardingTable = _entityMetadataManager.IsShardingTable(shardingEntityType);
            PaginationSequenceConfig dataSourceSequenceOrderConfig = null;
            PaginationSequenceConfig tableSequenceOrderConfig = null;
            if (isShardingDataSource)
            {
                var virtualDataSourceRoute = GetRoute(shardingEntityType);
                if (virtualDataSourceRoute.EnablePagination)
                {
                    dataSourceSequenceOrderConfig = virtualDataSourceRoute.PaginationMetadata.PaginationConfigs.OrderByDescending(o => o.AppendOrder)
                        .FirstOrDefault(o => o.AppendIfOrderNone && typeof(TEntity).ContainPropertyName(o.PropertyName));
                }

            }
            if (isShardingTable)
            {
                var virtualTable = _virtualTableManager.GetVirtualTable(shardingEntityType);
                if (virtualTable.EnablePagination)
                {
                    tableSequenceOrderConfig = virtualTable.PaginationMetadata.PaginationConfigs.OrderByDescending(o => o.AppendOrder)
                        .FirstOrDefault(o => o.AppendIfOrderNone && typeof(TEntity).ContainPropertyName(o.PropertyName));
                }
            }

            var useSequenceEnumeratorMergeEngine = isShardingDataSource && (dataSourceSequenceOrderConfig != null ||
                                                                            (isShardingTable &&
                                                                             !_streamMergeContext.IsCrossDataSource)) || (!isShardingDataSource && isShardingTable && tableSequenceOrderConfig != null);

            if (useSequenceEnumeratorMergeEngine)
            {
                return new AppendOrderSequenceStreamEnumerable<TShardingDbContext, TEntity>(_streamMergeContext, dataSourceSequenceOrderConfig, tableSequenceOrderConfig, _shardingPageManager.Current.RouteQueryResults);
            }


            return null;
        }

        private IStreamEnumerable<TEntity> DoOrderSequencePaginationEnumeratorStreamMergeEngine(Type shardingEntityType)
        {

            var orderCount = _streamMergeContext.Orders.Count();
            var primaryOrder = _streamMergeContext.Orders.First();
            var isShardingDataSource = _entityMetadataManager.IsShardingDataSource(shardingEntityType);
            var isShardingTable = _entityMetadataManager.IsShardingTable(shardingEntityType);
            PaginationSequenceConfig dataSourceSequenceOrderConfig = null;
            PaginationSequenceConfig tableSequenceOrderConfig = null;
            IVirtualDataSourceRoute virtualDataSourceRoute = null;
            IVirtualTable virtualTable = null;
            bool dataSourceUseReverse = true;
            bool tableUseReverse = true;
            if (isShardingDataSource)
            {
                virtualDataSourceRoute = GetRoute(shardingEntityType);
                if (virtualDataSourceRoute.EnablePagination)
                {
                    dataSourceSequenceOrderConfig = orderCount == 1 ? GetPaginationFullMatch(virtualDataSourceRoute.PaginationMetadata.PaginationConfigs, primaryOrder) : GetPaginationPrimaryMatch(virtualDataSourceRoute.PaginationMetadata.PaginationConfigs, primaryOrder);
                }

            }
            if (isShardingTable)
            {
                virtualTable = _virtualTableManager.GetVirtualTable(shardingEntityType);
                if (virtualTable.EnablePagination)
                {
                    tableSequenceOrderConfig = orderCount == 1 ? GetPaginationFullMatch(virtualTable.PaginationMetadata.PaginationConfigs, primaryOrder) : GetPaginationPrimaryMatch(virtualTable.PaginationMetadata.PaginationConfigs, primaryOrder);
                }
            }

            var useSequenceEnumeratorMergeEngine = isShardingDataSource && (dataSourceSequenceOrderConfig != null ||
                                                                            (isShardingTable &&
                                                                             !_streamMergeContext.IsCrossDataSource)) || (!isShardingDataSource && isShardingTable && tableSequenceOrderConfig != null);
            if (useSequenceEnumeratorMergeEngine)
            {
                return new SequenceStreamEnumerable<TShardingDbContext, TEntity>(_streamMergeContext, dataSourceSequenceOrderConfig, tableSequenceOrderConfig, _shardingPageManager.Current.RouteQueryResults, primaryOrder.IsAsc);
            }

            var total = _shardingPageManager.Current.RouteQueryResults.Sum(o => o.QueryResult);
            if (isShardingDataSource)
            {
                dataSourceUseReverse =
                    virtualDataSourceRoute.EnablePagination && EntityDataSourceUseReverseShardingPage(virtualDataSourceRoute, total);
            }
            if (isShardingTable)
            {
                tableUseReverse =
                    virtualTable.EnablePagination && EntityTableReverseShardingPage(virtualTable, total);
            }


            //skip过大reserve skip
            if (dataSourceUseReverse && tableUseReverse)
            {
                return new ReverseShardingStreamEnumerable<TEntity>(_streamMergeContext, total);
            }





            return null;
        }

        private bool EntityDataSourceUseReverseShardingPage(IVirtualDataSourceRoute virtualDataSourceRoute, long total)
        {
            if (virtualDataSourceRoute.PaginationMetadata.EnableReverseShardingPage && _streamMergeContext.Take.GetValueOrDefault() > 0)
            {
                if (virtualDataSourceRoute.PaginationMetadata.IsUseReverse(_streamMergeContext.Skip.GetValueOrDefault(), total))
                {
                    return true;
                }
            }
            return false;
        }
        private bool EntityTableReverseShardingPage(IVirtualTable virtualTable, long total)
        {
            if (virtualTable.PaginationMetadata.EnableReverseShardingPage && _streamMergeContext.Take.GetValueOrDefault() > 0)
            {
                if (virtualTable.PaginationMetadata.IsUseReverse(_streamMergeContext.Skip.GetValueOrDefault(), total))
                {
                    return true;
                }
            }
            return false;
        }

        private PaginationSequenceConfig GetPaginationFullMatch(ISet<PaginationSequenceConfig> paginationSequenceConfigs, PropertyOrder primaryOrder)
        {
            return paginationSequenceConfigs.FirstOrDefault(o => PaginationPrimaryMatch(o, primaryOrder));
        }
        private PaginationSequenceConfig GetPaginationPrimaryMatch(ISet<PaginationSequenceConfig> paginationSequenceConfigs, PropertyOrder primaryOrder)
        {
            return paginationSequenceConfigs.Where(o => o.PaginationMatchEnum.HasFlag(PaginationMatchEnum.PrimaryMatch)).FirstOrDefault(o => PaginationPrimaryMatch(o, primaryOrder));
        }

        private bool PaginationPrimaryMatch(PaginationSequenceConfig paginationSequenceConfig, PropertyOrder propertyOrder)
        {
            if (propertyOrder.PropertyExpression != paginationSequenceConfig.PropertyName)
                return false;

            if (paginationSequenceConfig.PaginationMatchEnum.HasFlag(PaginationMatchEnum.Owner))
                return _streamMergeContext.GetSingleShardingEntityType() == paginationSequenceConfig.OrderPropertyInfo.DeclaringType;
            if (paginationSequenceConfig.PaginationMatchEnum.HasFlag(PaginationMatchEnum.Named))
                return propertyOrder.PropertyExpression == paginationSequenceConfig.PropertyName;
            return false;
        }
    }
}