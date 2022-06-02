using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ShardingCore.Sharding.PaginationConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 01 September 2021 21:32:53
    * @Email: 326308290@qq.com
    */
    public class PaginationOrderPropertyBuilder
    {
        private readonly PaginationSequenceConfig _paginationSequenceConfig;

        public PaginationOrderPropertyBuilder(LambdaExpression orderPropertyExpression,PaginationMetadata metadata)
        {
            _paginationSequenceConfig = new PaginationSequenceConfig(orderPropertyExpression);
            metadata.PaginationConfigs.Add(_paginationSequenceConfig);
        }

        /// <summary>
        /// 使用哪个后缀比较
        /// 设置的比较器是asc的情况下
        /// </summary>
        /// <param name="routeComparer"></param>
        /// <returns></returns>
        public PaginationOrderPropertyBuilder UseRouteComparerAsc(IComparer<string> routeComparer)
        {

            _paginationSequenceConfig.RouteComparer= routeComparer ?? throw new ArgumentException(nameof(routeComparer));
            return this;
        }
        [Obsolete("plz use UseRouteComparerAsc")]
        public PaginationOrderPropertyBuilder UseRouteComparer(IComparer<string> routeComparer)
        {

            _paginationSequenceConfig.RouteComparer= routeComparer ?? throw new ArgumentException(nameof(routeComparer));
            return this;
        }
        /// <summary>
        /// 使用哪种比较方式
        /// </summary>
        /// <param name="paginationMatchEnum"></param>
        /// <returns></returns>
        public PaginationOrderPropertyBuilder UseQueryMatch(PaginationMatchEnum paginationMatchEnum)
        {
            _paginationSequenceConfig.PaginationMatchEnum = paginationMatchEnum;
            return this;
        }
        /// <summary>
        /// 如果查询没发现排序就将当前配置追加上去
        /// </summary>
        /// <param name="order">大于等于0生效,越大优先级越高</param>
        /// <param name="defAsc">默认asc还是desc</param>
        /// <returns></returns>
        public PaginationOrderPropertyBuilder UseAppendIfOrderNone(int order=0,bool defAsc=true)
        {
            _paginationSequenceConfig.AppendOrder = order;
            _paginationSequenceConfig.AppendAsc = defAsc;
            return this;
        }
    }
}