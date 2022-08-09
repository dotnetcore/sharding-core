using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.ShardingQueryableExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.Visitors.Selects;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Query.Sql;
#endif

namespace ShardingCore.Sharding.MergeContexts
{
    public sealed class QueryableRewriteEngine : IQueryableRewriteEngine
    {
        private readonly ILogger<QueryableRewriteEngine> _logger;
        private readonly bool _enableLogDebug;

        public QueryableRewriteEngine(ILogger<QueryableRewriteEngine> logger)
        {
            _logger = logger;
            _enableLogDebug = logger.IsEnabled(LogLevel.Debug);
        }

        public IRewriteResult GetRewriteQueryable(IMergeQueryCompilerContext mergeQueryCompilerContext,
            IParseResult parseResult)
        {
            var paginationContext = parseResult.GetPaginationContext();
            _logger.LogDebug($"rewrite queryable pagination context:[{paginationContext}]");
            var orderByContext = parseResult.GetOrderByContext();
            if (_enableLogDebug)
            {
                _logger.LogDebug($"rewrite queryable order by context:[{orderByContext}]");
            }

            var groupByContext = parseResult.GetGroupByContext();
            if (_enableLogDebug)
            {
                _logger.LogDebug(
                    $"rewrite queryable group by context:[{groupByContext.GroupExpression?.ShardingPrint()}]");
            }

            var selectContext = parseResult.GetSelectContext();
            if (_enableLogDebug)
            {
                _logger.LogDebug($"rewrite queryable select context:[{selectContext}]");
            }

            var skip = paginationContext.Skip;
            var take = paginationContext.Take;
            var orders = orderByContext.PropertyOrders;

            var combineQueryable = mergeQueryCompilerContext.GetQueryCombineResult().GetCombineQueryable();
            //去除分页,获取前Take+Skip数量
            var reWriteQueryable = combineQueryable;
            if (take.HasValue || skip.HasValue)
            {
                reWriteQueryable = reWriteQueryable.RemoveSkipAndTake();
            }


            if (take.HasValue)
            {
                if (skip.HasValue)
                {
                    reWriteQueryable = reWriteQueryable.ReSkip(0).ReTake(take.Value + skip.GetValueOrDefault());
                }
                else
                {
                    reWriteQueryable = reWriteQueryable.ReTake(take.Value + skip.GetValueOrDefault());
                }
            }

            //包含group by select必须包含group by字段其余的order字段需要在内存中实现
            if (groupByContext.GroupExpression != null)
            {
                //group字段不可以为空
                var selectGroupKeyProperties =
                    selectContext.SelectProperties.Where(o => !(o is SelectAggregateProperty)).ToArray();
                if (selectGroupKeyProperties.IsEmpty())
                {
                    throw new ShardingCoreInvalidOperationException(
                        "group by select object must contains group by key value");
                }


                if (orders.IsEmpty())
                {
                    groupByContext.GroupMemoryMerge = false;
                    var sort = string.Join(",", selectGroupKeyProperties.Select(o => $"{o.PropertyName} asc"));
                    reWriteQueryable = reWriteQueryable.RemoveAnyOrderBy().OrderWithExpression(sort, null);
                    foreach (var orderProperty in selectGroupKeyProperties)
                    {
                        orders.AddLast(new PropertyOrder(orderProperty.PropertyName, true, orderProperty.OwnerType));
                    }
                }
                else
                {
                    var groupKeys = selectGroupKeyProperties.Select(o => o.PropertyName).ToHashSet();
                    bool groupMemoryMerge = false;
                    foreach (var propertyOrder in orders)
                    {
                        groupByContext.PropertyOrders.Add(propertyOrder);
                        if (!groupMemoryMerge && !groupKeys.IsEmpty())
                        {
                            if (!groupKeys.Contains(propertyOrder.PropertyExpression))
                            {
                                groupMemoryMerge = true;
                            }

                            groupKeys.Remove(propertyOrder.PropertyExpression);
                        }
                    }

                    //判断是否优先group key排序如果不是就是要内存聚合
                    groupByContext.GroupMemoryMerge = groupMemoryMerge;
                    if (groupByContext.GroupMemoryMerge)
                    {
                        if (groupByContext.GroupMemoryMerge)
                        {
                            var sort = string.Join(",", selectGroupKeyProperties.Select(o => $"{o.PropertyName} asc"));
                            reWriteQueryable = reWriteQueryable.RemoveAnyOrderBy().OrderWithExpression(sort, null);
                        }

                        orders.Clear();
                        foreach (var orderProperty in selectGroupKeyProperties)
                        {
                            orders.AddLast(new PropertyOrder(orderProperty.PropertyName, true,
                                orderProperty.OwnerType));
                        }
                    }
                }

                // else if (!mergeQueryCompilerContext.UseUnionAllMerge())
                // {
                //     //将查询的属性转换成order by 并且order和select的未聚合查询必须一致
                //     // var selectProperties = selectContext.SelectProperties.Where(o => !(o is SelectAggregateProperty));
                //
                //     // if (orders.Count() != selectProperties.Count())
                //     //     throw new ShardingCoreInvalidOperationException("group by query order items not equal select un-aggregate items");
                //     // var os = orders.Select(o => o.PropertyExpression).ToList();
                //     // var ss = selectProperties.Select(o => o.PropertyName).ToList();
                //     // for (int i = 0; i < os.Count(); i++)
                //     // {
                //     //     if (!os[i].Equals(ss[i]))
                //     //         throw new ShardingCoreInvalidOperationException($"group by query order items not equal select un-aggregate items: order:[{os[i]}],select:[{ss[i]}");
                //     // }
                //
                // }
                if (selectContext.HasAverage())
                {
                    var averageSelectProperties =
                        selectContext.SelectProperties.OfType<SelectAverageProperty>().ToList();
                    var selectAggregateProperties = selectContext.SelectProperties.OfType<SelectAggregateProperty>()
                        .Where(o => !(o is SelectAverageProperty)).ToList();
                    foreach (var averageSelectProperty in averageSelectProperties)
                    {
                        var selectCountProperty =
                            selectAggregateProperties.FirstOrDefault(o => o is SelectCountProperty selectCountProperty);
                        if (null != selectCountProperty)
                        {
                            averageSelectProperty.BindCountProperty(selectCountProperty.Property);
                        }

                        var selectSumProperty = selectAggregateProperties.FirstOrDefault(o =>
                            o is SelectSumProperty selectSumProperty &&
                            selectSumProperty.FromProperty == averageSelectProperty.FromProperty);
                        if (selectSumProperty != null)
                        {
                            averageSelectProperty.BindSumProperty(selectSumProperty.Property);
                        }

                        if (averageSelectProperty.CountProperty == null && averageSelectProperty.SumProperty == null)
                            throw new ShardingCoreInvalidOperationException(
                                $"use aggregate function average error,not found count aggregate function and not found sum aggregate function that property name same as average aggregate function property name:[{averageSelectProperty.FromProperty?.Name}]");
                    }
                }
                //else
                //{
                //    //将查询的属性转换成order by 并且order和select的未聚合查询必须一致
                //    var selectProperties = selectContext.SelectProperties.Where(o => !(o is SelectAggregateProperty));

                //    if (orders.Count() != selectProperties.Count())
                //        throw new ShardingCoreInvalidOperationException("group by query order items not equal select un-aggregate items");
                //    var os = orders.Select(o => o.PropertyExpression).ToList();
                //    var ss = selectProperties.Select(o => o.PropertyName).ToList();
                //    for (int i = 0; i < os.Count(); i++)
                //    {
                //        if (!os[i].Equals(ss[i]))
                //            throw new ShardingCoreInvalidOperationException($"group by query order items not equal select un-aggregate items: order:[{os[i]}],select:[{ss[i]}");
                //    }
                //}
            }

            if (mergeQueryCompilerContext.UseUnionAllMerge())
            {
                if (!mergeQueryCompilerContext.GetShardingDbContext().SupportUnionAllMerge())
                {
                    throw new ShardingCoreException(
                        $"if use {nameof(EntityFrameworkShardingQueryableExtension.UseUnionAllMerge)} plz rewrite {nameof(IQuerySqlGeneratorFactory)} with {nameof(IUnionAllMergeQuerySqlGeneratorFactory)} and {nameof(IQueryCompiler)} with {nameof(IUnionAllMergeQueryCompiler)}");
                }
            }

            return new RewriteResult(combineQueryable, reWriteQueryable);
        }
    }
}