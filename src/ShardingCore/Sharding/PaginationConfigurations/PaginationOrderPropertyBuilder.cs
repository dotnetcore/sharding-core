using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
        private readonly LambdaExpression _orderPropertyExpression;
        private IComparer<string> _tailComparer;
        private PaginationMatchEnum _paginationMatchEnum;
        private PropertyInfo _orderPropertyInfo;

        public PaginationOrderPropertyBuilder(LambdaExpression orderPropertyExpression)
        {
            _orderPropertyExpression = orderPropertyExpression;
            _orderPropertyInfo = orderPropertyExpression.GetPropertyAccess();
        }

        /// <summary>
        /// 使用哪个后缀比较
        /// </summary>
        /// <param name="tailComparer"></param>
        /// <returns></returns>
        public PaginationOrderPropertyBuilder UseTailCompare(IComparer<string> tailComparer)
        {
            _tailComparer = tailComparer ?? throw new ArgumentException(nameof(tailComparer));
            return this;
        }
        /// <summary>
        /// 使用哪种比较方式
        /// </summary>
        /// <param name="paginationMatchEnum"></param>
        /// <returns></returns>
        public PaginationOrderPropertyBuilder UseQueryMatch(PaginationMatchEnum paginationMatchEnum)
        {
            _paginationMatchEnum = paginationMatchEnum;
            return this;
        }
    }
}