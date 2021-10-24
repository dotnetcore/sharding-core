using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
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
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync;

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
        private readonly StreamMergeContext<TEntity> _streamMergeContext;
        private readonly IShardingPageManager _shardingPageManager;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        private EnumeratorStreamMergeEngineFactory(StreamMergeContext<TEntity> streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
            _shardingPageManager = ShardingContainer.GetService<IShardingPageManager>();
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
            _virtualDataSource = ShardingContainer.GetService<IVirtualDataSource<TShardingDbContext>>();
        }

        public static EnumeratorStreamMergeEngineFactory<TShardingDbContext, TEntity> Create(StreamMergeContext<TEntity> streamMergeContext)
        {
            return new EnumeratorStreamMergeEngineFactory<TShardingDbContext, TEntity>(streamMergeContext);
        }

        public IEnumeratorStreamMergeEngine<TEntity> GetMergeEngine()
        {
            //本次查询没有跨库没有跨表就可以直接执行
            if (!_streamMergeContext.IsCrossDataSource&&!_streamMergeContext.IsCrossTable)
            {
                return new SingleQueryEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity>(_streamMergeContext);
            }

            //未开启系统分表或者本次查询涉及多张分表
            if (_streamMergeContext.IsPaginationQuery() && _streamMergeContext.IsSupportPaginationQuery() && _shardingPageManager.Current != null)
            {
                //获取虚拟表判断是否启用了分页配置
                var shardingEntityType = _streamMergeContext.QueryEntities.FirstOrDefault(o => o.IsShardingDataSource() || o.IsShardingTable());
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


            return new DefaultShardingEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity>(_streamMergeContext);
        }

        private IEnumeratorStreamMergeEngine<TEntity> DoNoOrderAppendEnumeratorStreamMergeEngine(Type shardingEntityType)
        {

            var isShardingDataSource = shardingEntityType.IsShardingDataSource();
            var isShardingTable = shardingEntityType.IsShardingTable();
            PaginationSequenceConfig dataSourceSequenceOrderConfig = null;
            PaginationSequenceConfig tableSequenceOrderConfig = null;
            if (isShardingDataSource)
            {
                var virtualDataSourceRoute = _virtualDataSource.GetRoute(shardingEntityType);
                if (virtualDataSourceRoute.EnablePagination)
                {
                    dataSourceSequenceOrderConfig = virtualDataSourceRoute.PaginationMetadata.PaginationConfigs.OrderByDescending(o => o.AppendOrder)
                        .FirstOrDefault(o => o.AppendIfOrderNone && typeof(TEntity).ContainPropertyName(o.PropertyName) && PaginationMatch(o));
                }

            }
            if (isShardingTable)
            {
                var virtualTable = _virtualTableManager.GetVirtualTable(shardingEntityType);
                if (virtualTable.EnablePagination)
                {
                    tableSequenceOrderConfig = virtualTable.PaginationMetadata.PaginationConfigs.OrderByDescending(o => o.AppendOrder)
                        .FirstOrDefault(o => o.AppendIfOrderNone && typeof(TEntity).ContainPropertyName(o.PropertyName) && PaginationMatch(o));
                }
            }
            if (dataSourceSequenceOrderConfig != null || tableSequenceOrderConfig != null)
            {
                return new AppenOrderSequenceEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity>(_streamMergeContext, dataSourceSequenceOrderConfig, tableSequenceOrderConfig, _shardingPageManager.Current.RouteQueryResults);
            }

            return null;
        }

        private IEnumeratorStreamMergeEngine<TEntity> DoOrderSequencePaginationEnumeratorStreamMergeEngine(Type shardingEntityType)
        {

            var orderCount = _streamMergeContext.Orders.Count();
            var primaryOrder = _streamMergeContext.Orders.First();
            var isShardingDataSource = shardingEntityType.IsShardingDataSource();
            var isShardingTable = shardingEntityType.IsShardingTable();
            PaginationSequenceConfig dataSourceSequenceOrderConfig = null;
            PaginationSequenceConfig tableSequenceOrderConfig = null;
            IVirtualDataSourceRoute virtualDataSourceRoute = null;
            IVirtualTable virtualTable = null;
            bool dataSourceUseReverse = true;
            bool tableUseReverse = true;
            if (isShardingDataSource)
            {
                virtualDataSourceRoute = _virtualDataSource.GetRoute(shardingEntityType);
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
            if (dataSourceSequenceOrderConfig != null || tableSequenceOrderConfig != null)
            {
                return new SequenceEnumeratorAsyncStreamMergeEngine<TShardingDbContext, TEntity>(_streamMergeContext, dataSourceSequenceOrderConfig, tableSequenceOrderConfig, _shardingPageManager.Current.RouteQueryResults, primaryOrder.IsAsc);
            }

            var total = _shardingPageManager.Current.RouteQueryResults.Sum(o => o.QueryResult);
            if (isShardingDataSource&& virtualDataSourceRoute.EnablePagination)
            {
                dataSourceUseReverse =
                    EntityDataSourceUseReverseShardingPage(virtualDataSourceRoute, total);
            }
            if (isShardingTable && virtualTable.EnablePagination)
            {
                tableUseReverse =
                    EntityTableReverseShardingPage(virtualTable, total);
            }
            

            //skip过大reserve skip
            if (dataSourceUseReverse && tableUseReverse)
            {
                return new ReverseShardingEnumeratorAsyncStreamMergeEngine<TEntity>(_streamMergeContext, total);
            }





            return null;
        }

        private bool EntityDataSourceUseReverseShardingPage( IVirtualDataSourceRoute virtualDataSourceRoute,long total)
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
        private bool EntityTableReverseShardingPage( IVirtualTable virtualTable, long total)
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
            return paginationSequenceConfigs.Where(o => !o.PaginationMatchEnum.HasFlag(PaginationMatchEnum.PrimaryMatch)).FirstOrDefault(o => PaginationPrimaryMatch(o, primaryOrder));
        }
        private PaginationSequenceConfig GetPaginationPrimaryMatch(ISet<PaginationSequenceConfig> paginationSequenceConfigs, PropertyOrder primaryOrder)
        {
            return paginationSequenceConfigs.Where(o => o.PaginationMatchEnum.HasFlag(PaginationMatchEnum.PrimaryMatch)).FirstOrDefault(o => PaginationPrimaryMatch(o, primaryOrder));
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