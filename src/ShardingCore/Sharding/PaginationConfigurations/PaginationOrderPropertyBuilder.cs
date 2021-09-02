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
        private readonly PaginationConfig _paginationConfig;

        public PaginationOrderPropertyBuilder(LambdaExpression orderPropertyExpression,PaginationMetadata metadata)
        {
            _paginationConfig = new PaginationConfig(orderPropertyExpression);
            metadata.PaginationConfigs.Add(_paginationConfig);
        }

        /// <summary>
        /// 使用哪个后缀比较
        /// </summary>
        /// <param name="tailComparer"></param>
        /// <returns></returns>
        public PaginationOrderPropertyBuilder UseTailCompare(IComparer<string> tailComparer)
        {

            _paginationConfig.TailComparer= tailComparer ?? throw new ArgumentException(nameof(tailComparer));
            return this;
        }
        /// <summary>
        /// 使用哪种比较方式
        /// </summary>
        /// <param name="paginationMatchEnum"></param>
        /// <returns></returns>
        public PaginationOrderPropertyBuilder UseQueryMatch(PaginationMatchEnum paginationMatchEnum)
        {
            _paginationConfig.PaginationMatchEnum = paginationMatchEnum;
            return this;
        }
        /// <summary>
        /// 如果查询没发现排序就将当前配置追加上去
        /// </summary>
        /// <returns></returns>
        public PaginationOrderPropertyBuilder UseAppendIfOrderNone()
        {
            _paginationConfig.AppendIfOrderNone = true;
            return this;
        }
    }
}