using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Sharding.StreamMergeEngines;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Base;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.Enumerables
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 16:29:06
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class SequenceStreamEnumerable<TShardingDbContext, TEntity> : AbstractStreamEnumerable<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly PaginationSequenceConfig _dataSourceSequenceMatchOrderConfig;
        private readonly PaginationSequenceConfig _tableSequenceMatchOrderConfig;
        private readonly ICollection<RouteQueryResult<long>> _routeQueryResults;
        private readonly bool _isAsc;
        public SequenceStreamEnumerable(StreamMergeContext streamMergeContext, PaginationSequenceConfig dataSourceSequenceMatchOrderConfig, PaginationSequenceConfig tableSequenceMatchOrderConfig, ICollection<RouteQueryResult<long>> routeQueryResults, bool isAsc) 
            : base(streamMergeContext)
        {
            _dataSourceSequenceMatchOrderConfig = dataSourceSequenceMatchOrderConfig;
            _tableSequenceMatchOrderConfig = tableSequenceMatchOrderConfig;
            _routeQueryResults = routeQueryResults;
            _isAsc = isAsc;
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return SequenceStreamMergeCombine.Instance;
        }

        protected override IEnumerable<ISqlRouteUnit> GetDefaultSqlRouteUnits()
        {
            var skip = GetStreamMergeContext().Skip.GetValueOrDefault();
            if (skip < 0)
                throw new ShardingCoreException("skip must ge 0");

            var take = GetStreamMergeContext().Take;
            if (take.HasValue && take.Value <= 0)
                throw new ShardingCoreException("take must gt 0");
            //分库是主要排序
            var dataSourceOrderMain = _dataSourceSequenceMatchOrderConfig != null;
            var sortRouteResults = _routeQueryResults.Select(o => new
            {
                DataSourceName = o.DataSourceName,
                Tail = o.TableRouteResult.ReplaceTables.First().Tail,
                RouteQueryResult = o
            });
            if (dataSourceOrderMain)
            {
                //是否有两级排序
                var useThenBy = _tableSequenceMatchOrderConfig != null;
                if (_isAsc)
                {
                    sortRouteResults = sortRouteResults.OrderBy(o => o.DataSourceName,
                        _dataSourceSequenceMatchOrderConfig.RouteComparer).ThenByIf(o => o.Tail, useThenBy, _tableSequenceMatchOrderConfig.RouteComparer);
                }
                else
                {
                    sortRouteResults = sortRouteResults.OrderByDescending(o => o.DataSourceName,
                        _dataSourceSequenceMatchOrderConfig.RouteComparer).ThenByDescendingIf(o => o.Tail, useThenBy, _tableSequenceMatchOrderConfig.RouteComparer);
                }
            }
            else
            {
                if (_isAsc)
                {
                    sortRouteResults =
                        sortRouteResults.OrderBy(o => o.Tail, _tableSequenceMatchOrderConfig.RouteComparer);
                }
                else
                {
                    sortRouteResults =
                        sortRouteResults.OrderByDescending(o => o.Tail, _tableSequenceMatchOrderConfig.RouteComparer);
                }
            }


            var sequenceResults = new SequencePaginationList(sortRouteResults.Select(o => o.RouteQueryResult)).Skip(skip).Take(take).ToList();
            return  sequenceResults.Select(sequenceResult => new SqlSequenceRouteUnit(sequenceResult));

        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor0(bool async)
        {
            return new SequenceEnumeratorExecutor<TEntity>(GetStreamMergeContext(), GetStreamMergeCombine(), async);
        }
    }
}
