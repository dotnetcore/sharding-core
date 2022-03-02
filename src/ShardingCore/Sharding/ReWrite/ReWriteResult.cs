using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.Internal.Visitors.Selects;
using ShardingCore.Sharding.MergeContexts;

namespace ShardingCore.Core.Internal.StreamMerge.ReWrite
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 23:45:29
* @Email: 326308290@qq.com
*/
    internal class ReWriteResult<T>
    {
        public ReWriteResult(IQueryable<T> originalQueryable, IQueryable<T> reWriteQueryable, int? skip, int? take, IEnumerable<PropertyOrder> orders, SelectContext selectContext, GroupByContext groupByContext)
        {
            OriginalQueryable = originalQueryable;
            ReWriteQueryable = reWriteQueryable;
            Skip = skip;
            Take = take;
            Orders = orders;
            SelectContext = selectContext;
            GroupByContext = groupByContext;
        }

        public IQueryable<T> OriginalQueryable { get; }
        public IQueryable<T> ReWriteQueryable { get; }
        public int? Skip { get; }
        public int? Take { get; }
        public IEnumerable<PropertyOrder> Orders { get; }
        public SelectContext SelectContext { get; }
        public GroupByContext GroupByContext { get; }
    }
}