using System.Collections.Generic;
using System.Linq;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.Visitors.Selects;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 13:13:45
* @Email: 326308290@qq.com
*/
    internal class ExtraEntry
    {
        public ExtraEntry(int? skip, int? take, IEnumerable<PropertyOrder> orders, SelectContext selectContext, GroupByContext groupByContext)
        {
            Skip = skip;
            Take = take;
            Orders = orders;
            SelectContext = selectContext;
            GroupByContext = groupByContext;
        }

        public int? Skip { get; }
        public int? Take { get; }
        public IEnumerable<PropertyOrder> Orders { get; }
        public SelectContext SelectContext { get; }
        public GroupByContext GroupByContext { get; }


        public void ProcessGroupBySelectProperties()
        {
            if (GroupByContext == null)
                return;
            if (!SelectContext.HasAverage())
                return;
            var averageSelectProperties = SelectContext.SelectProperties.OfType<SelectAverageProperty>().ToList();
            var selectAggregateProperties = SelectContext.SelectProperties.OfType<SelectAggregateProperty>().Where(o=>!(o is SelectAverageProperty)).ToList();
            foreach (var averageSelectProperty in averageSelectProperties)
            {
                var selectCountProperty = selectAggregateProperties.FirstOrDefault(o=>o is SelectCountProperty  selectCountProperty);
                if (null != selectCountProperty)
                {
                    averageSelectProperty.BindCountProperty(selectCountProperty.Property);
                }
                var selectSumProperty = selectAggregateProperties.FirstOrDefault(o => o is SelectSumProperty selectSumProperty&& selectSumProperty.FromProperty== averageSelectProperty.FromProperty);
                if (selectSumProperty != null)
                {
                    averageSelectProperty.BindSumProperty(selectSumProperty.Property);
                }

                if (averageSelectProperty.CountProperty == null && averageSelectProperty.SumProperty == null)
                    throw new ShardingCoreInvalidOperationException(
                        $"use aggregate function average error,not found count aggregate function and not found sum aggregate function that property name same as average aggregate function property name:[{averageSelectProperty.FromProperty?.Name}]");
            }

        }

    }
}