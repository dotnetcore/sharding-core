using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
        public IQueryable GetRewriteQueryable(IMergeQueryCompilerContext mergeQueryCompilerContext, IParseResult parseResult)
        {
            var paginationContext = parseResult.GetPaginationContext();
            var orderByContext = parseResult.GetOrderByContext();
            var groupByContext = parseResult.GetGroupByContext();
            var selectContext = parseResult.GetSelectContext();
            var skip = paginationContext.Skip;
            var take = paginationContext.Take;
            var orders = orderByContext.PropertyOrders;

            //去除分页,获取前Take+Skip数量
            var reWriteQueryable = mergeQueryCompilerContext.GetQueryCombineResult().GetCombineQueryable();
            if (take.HasValue)
            {
                reWriteQueryable = reWriteQueryable.RemoveTake();
            }

            if (skip.HasValue)
            {
                reWriteQueryable = reWriteQueryable.RemoveSkip();
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
            //包含group by
            if (groupByContext.GroupExpression != null)
            {
                if (orders.IsEmpty())
                {
                    //将查询的属性转换成order by
                    var selectProperties = selectContext.SelectProperties.Where(o => !(o is SelectAggregateProperty)).ToArray();
                    if (selectProperties.IsNotEmpty())
                    {
                        var sort = string.Join(",", selectProperties.Select(o => $"{o.PropertyName} asc"));
                        reWriteQueryable = reWriteQueryable.OrderWithExpression(sort, null);
                        foreach (var orderProperty in selectProperties)
                        {
                            orders.AddLast(new PropertyOrder(orderProperty.PropertyName, true, orderProperty.OwnerType));
                        }
                    }
                }
                else if (!mergeQueryCompilerContext.UseUnionAllMerge())
                {
                    //将查询的属性转换成order by 并且order和select的未聚合查询必须一致
                    var selectProperties = selectContext.SelectProperties.Where(o => !(o is SelectAggregateProperty));

                    if (orders.Count() != selectProperties.Count())
                        throw new ShardingCoreInvalidOperationException("group by query order items not equal select un-aggregate items");
                    var os = orders.Select(o => o.PropertyExpression).ToList();
                    var ss = selectProperties.Select(o => o.PropertyName).ToList();
                    for (int i = 0; i < os.Count(); i++)
                    {
                        if (!os[i].Equals(ss[i]))
                            throw new ShardingCoreInvalidOperationException($"group by query order items not equal select un-aggregate items: order:[{os[i]}],select:[{ss[i]}");
                    }

                }
                if (selectContext.HasAverage())
                {
                    var averageSelectProperties = selectContext.SelectProperties.OfType<SelectAverageProperty>().ToList();
                    var selectAggregateProperties = selectContext.SelectProperties.OfType<SelectAggregateProperty>().Where(o => !(o is SelectAverageProperty)).ToList();
                    foreach (var averageSelectProperty in averageSelectProperties)
                    {
                        var selectCountProperty = selectAggregateProperties.FirstOrDefault(o => o is SelectCountProperty selectCountProperty);
                        if (null != selectCountProperty)
                        {
                            averageSelectProperty.BindCountProperty(selectCountProperty.Property);
                        }
                        var selectSumProperty = selectAggregateProperties.FirstOrDefault(o => o is SelectSumProperty selectSumProperty && selectSumProperty.FromProperty == averageSelectProperty.FromProperty);
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

            if (mergeQueryCompilerContext.UseUnionAllMerge() & !mergeQueryCompilerContext.GetShardingDbContext().SupportUnionAllMerge())
            {
                throw new ShardingCoreException(
                $"if use {nameof(EntityFrameworkShardingQueryableExtension.UseUnionAllMerge)} plz rewrite {nameof(IQuerySqlGeneratorFactory)} with {nameof(IUnionAllMergeQuerySqlGeneratorFactory)} and {nameof(IQueryCompiler)} with {nameof(IUnionAllMergeQueryCompiler)}");

            }

            return reWriteQueryable;
        }
    }
}
