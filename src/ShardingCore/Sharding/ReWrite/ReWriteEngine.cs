using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.Visitors.Selects;

namespace ShardingCore.Core.Internal.StreamMerge.ReWrite
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 23:44:24
* @Email: 326308290@qq.com
*/
    internal class ReWriteEngine<T>
    {
        private readonly IQueryable<T> _queryable;

        public ReWriteEngine(IQueryable<T> queryable)
        {
            _queryable = queryable;
        }

        public ReWriteResult<T> ReWrite()
        {
            var extraEntry = _queryable.GetExtraEntry();
            var skip = extraEntry.Skip;
            var take = extraEntry.Take;
            var orders = extraEntry.Orders ?? Enumerable.Empty<PropertyOrder>();
            
            //去除分页,获取前Take+Skip数量
            var reWriteQueryable = _queryable;
            if (take.HasValue)
            {
                reWriteQueryable = reWriteQueryable.RemoveTake().As<IQueryable<T>>();
            }

            if (skip.HasValue)
            {
                reWriteQueryable = reWriteQueryable.RemoveSkip().As<IQueryable<T>>();
            }

            if (take.HasValue)
            {
                if (skip.HasValue)
                {
                    reWriteQueryable = reWriteQueryable.Skip(0).Take(take.Value + skip.GetValueOrDefault());
                }
                else
                {
                    reWriteQueryable = reWriteQueryable.Take(take.Value + skip.GetValueOrDefault());
                }
            }
            //包含group by
            if (extraEntry.GroupByContext.GroupExpression != null)
            {
                if (orders.IsEmpty())
                {
                    //将查询的属性转换成order by
                    var selectProperties = extraEntry.SelectContext.SelectProperties.Where(o => !(o is SelectAggregateProperty));
                    if (selectProperties.IsNotEmpty())
                    {
                        var sort = string.Join(",",selectProperties.Select(o=>$"{o.PropertyName} asc"));
                        reWriteQueryable = reWriteQueryable.OrderWithExpression(sort,null);
                        var reWriteOrders = new List<PropertyOrder>(selectProperties.Count());
                        foreach (var orderProperty in selectProperties)
                        {
                            reWriteOrders.Add(new PropertyOrder(orderProperty.PropertyName,true, orderProperty.OwnerType));
                        }
                        orders = reWriteOrders;
                    }
                }
                else
                {
                    //将查询的属性转换成order by 并且order和select的未聚合查询必须一致
                    var selectProperties = extraEntry.SelectContext.SelectProperties.Where(o => !(o is SelectAggregateProperty));

                    if (orders.Count() != selectProperties.Count())
                        throw new ShardingCoreInvalidOperationException("group by query order items not equal select un-aggregate items");
                    var os=orders.Select(o => o.PropertyExpression).ToList();
                    var ss = selectProperties.Select(o => o.PropertyName).ToList();
                    for (int i = 0; i < os.Count(); i++)
                    {
                        if(!os[i].Equals(ss[i]))
                            throw new ShardingCoreInvalidOperationException($"group by query order items not equal select un-aggregate items: order:[{os[i]}],select:[{ss[i]}");
                    }
                }
            }
            return new ReWriteResult<T>(_queryable,reWriteQueryable,skip,take,orders,extraEntry.SelectContext,extraEntry.GroupByContext);
        }
        
    }
}