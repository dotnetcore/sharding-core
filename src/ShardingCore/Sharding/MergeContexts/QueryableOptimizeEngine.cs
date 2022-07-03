using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Sharding.EntityQueryConfigurations;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.MergeContexts
{
    public sealed class QueryableOptimizeEngine : IQueryableOptimizeEngine
    {
        private readonly ITableRouteManager _tableRouteManager;

        public QueryableOptimizeEngine(ITableRouteManager tableRouteManager)
        {
            _tableRouteManager = tableRouteManager;
        }

        public IOptimizeResult Optimize(IMergeQueryCompilerContext mergeQueryCompilerContext, IParseResult parseResult,
            IRewriteResult rewriteResult)
        {
            var shardingDbContext = mergeQueryCompilerContext.GetShardingDbContext();
            var maxParallelExecuteCount =
                shardingDbContext.GetVirtualDataSource().ConfigurationParams.MaxQueryConnectionsLimit;
            var connectionMode = shardingDbContext.GetVirtualDataSource().ConfigurationParams.ConnectionMode;
            IComparer<string> shardingTailComparer = Comparer<string>.Default;
            bool sameWithTailComparer = true;
            bool sequenceQuery = false;
            if (mergeQueryCompilerContext.IsSingleShardingEntityQuery() && mergeQueryCompilerContext.IsCrossTable() &&
                !mergeQueryCompilerContext.UseUnionAllMerge())
            {
                var singleShardingEntityType = mergeQueryCompilerContext.GetSingleShardingEntityType();
                var tableRoute = _tableRouteManager.GetRoute(singleShardingEntityType);
                if (tableRoute.EnableEntityQuery)
                {
                    if (tableRoute.EntityQueryMetadata.DefaultTailComparer != null)
                    {
                        shardingTailComparer = tableRoute.EntityQueryMetadata.DefaultTailComparer;
                    }

                    sameWithTailComparer = tableRoute.EntityQueryMetadata.DefaultTailComparerNeedReverse;
                    string methodName = mergeQueryCompilerContext.IsEnumerableQuery()
                        ? EntityQueryMetadata.QUERY_ENUMERATOR
                        : ((MethodCallExpression)mergeQueryCompilerContext.GetQueryExpression()).Method.Name;

                    if (tableRoute.EntityQueryMetadata.TryGetConnectionsLimit(methodName, out var limit))
                    {
                        maxParallelExecuteCount = Math.Min(limit, maxParallelExecuteCount);
                    }

                    var isSequence = mergeQueryCompilerContext.IsSequence();
                    var sameWithShardingComparer = mergeQueryCompilerContext.SameWithShardingComparer();
                    if (isSequence.HasValue && sameWithShardingComparer.HasValue)
                    {
                        sequenceQuery = isSequence.Value;
                        sameWithTailComparer = sameWithShardingComparer.Value;
                    }
                    else
                    {
                        if (TryGetSequenceQuery(parseResult, singleShardingEntityType, tableRoute, methodName,
                                out var tailComparerIsAsc))
                        {
                            sequenceQuery = true;
                            if (!tailComparerIsAsc)
                            {
                                sameWithTailComparer = !sameWithTailComparer;
                            }
                        }
                    }
                }
            }

            maxParallelExecuteCount =
                mergeQueryCompilerContext.GetMaxQueryConnectionsLimit() ?? maxParallelExecuteCount;

            connectionMode = mergeQueryCompilerContext.GetConnectionMode() ?? connectionMode;
            return new OptimizeResult(maxParallelExecuteCount, connectionMode, sequenceQuery, sameWithTailComparer,
                shardingTailComparer);
        }

        /// <summary>
        /// 是否需要判断order
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="propertyOrders"></param>
        /// <returns></returns>
        private bool EffectOrder(string methodName, PropertyOrder[] propertyOrders)
        {
            if ((methodName == null ||
                 nameof(Queryable.First) == methodName ||
                 nameof(Queryable.FirstOrDefault) == methodName ||
                 nameof(Queryable.Last) == methodName ||
                 nameof(Queryable.LastOrDefault) == methodName ||
                 nameof(Queryable.Single) == methodName ||
                 nameof(Queryable.SingleOrDefault) == methodName ||
                 EntityQueryMetadata.QUERY_ENUMERATOR == methodName) &&
                propertyOrders.Length > 0)
                return true;
            return false;
        }

        /// <summary>
        /// 尝试获取当前方法是否采用顺序查询,如果有先判断排序没有的情况下判断默认
        /// </summary>
        /// <param name="parseResult"></param>
        /// <param name="singleShardingEntityType"></param>
        /// <param name="tableRoute"></param>
        /// <param name="methodName"></param>
        /// <param name="tailComparerIsAsc"></param>
        /// <returns></returns>
        private bool TryGetSequenceQuery(IParseResult parseResult, Type singleShardingEntityType,
            IVirtualTableRoute tableRoute, string methodName, out bool tailComparerIsAsc)
        {
            var propertyOrders = parseResult.GetOrderByContext().PropertyOrders.ToArray();
            var effectOrder = EffectOrder(methodName, propertyOrders);

            if (effectOrder)
            {
                var primaryOrder = propertyOrders[0];
                //不是多级order 
                var primaryOrderPropertyName = primaryOrder.PropertyExpression;
                if (!primaryOrderPropertyName.Contains("."))
                {
                    if (tableRoute.EnableEntityQuery && tableRoute.EntityQueryMetadata.TryContainsComparerOrder(
                                                           primaryOrderPropertyName, out var seqQueryOrderMatch)
                                                       && (primaryOrder.OwnerType == singleShardingEntityType ||
                                                           seqQueryOrderMatch.OrderMatch.HasFlag(
                                                               SeqOrderMatchEnum.Named))) //要么必须是当前对象查询要么就是名称一样
                    {
                        tailComparerIsAsc = seqQueryOrderMatch.IsSameAsShardingTailComparer
                            ? primaryOrder.IsAsc
                            : !primaryOrder.IsAsc;
                        //如果是获取最后一个还需要再次翻转
                        if (nameof(Queryable.Last) == methodName || nameof(Queryable.LastOrDefault) == methodName)
                        {
                            tailComparerIsAsc = !tailComparerIsAsc;
                        }

                        return true;
                    }
                }

                tailComparerIsAsc = true;
                return false;
            }

            if (tableRoute.EnableEntityQuery && methodName != null &&
                tableRoute.EntityQueryMetadata.TryGetDefaultSequenceQueryTrip(methodName, out var defaultAsc))
            {
                tailComparerIsAsc = defaultAsc;
                return true;
            }

            //Max和Min
            if (nameof(Queryable.Max) == methodName || nameof(Queryable.Min) == methodName)
            {
                //如果是max或者min
                if (tableRoute.EnableEntityQuery && parseResult.GetSelectContext().SelectProperties.Count == 1 &&
                    tableRoute.EntityQueryMetadata.TryContainsComparerOrder(
                        parseResult.GetSelectContext().SelectProperties[0].PropertyName, out var seqQueryOrderMatch)
                    && (parseResult.GetSelectContext().SelectProperties[0].OwnerType == singleShardingEntityType ||
                        seqQueryOrderMatch.OrderMatch.HasFlag(SeqOrderMatchEnum.Named)))
                {
                    tailComparerIsAsc = seqQueryOrderMatch.IsSameAsShardingTailComparer
                        ? nameof(Queryable.Min) == methodName
                        : nameof(Queryable.Max) == methodName;
                    return true;
                }
            }

            tailComparerIsAsc = true;
            return false;
        }
    }
}