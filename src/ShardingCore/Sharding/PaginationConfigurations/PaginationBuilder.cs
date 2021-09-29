using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core;
using ShardingCore.Sharding.PaginationConfigurations.MultiQueryPagination;

namespace ShardingCore.Sharding.PaginationConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/1 17:33:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class PaginationBuilder<TEntity> where TEntity:class
    {
        private readonly PaginationMetadata  _metadata;

        public PaginationBuilder(PaginationMetadata metadata)
        {
            _metadata = metadata;
        }
        /// <summary>
        /// 分页顺序
        /// </summary>
        /// <param name="orderPropertyExpression"></param>
        /// <typeparam name="TProperty"></typeparam>
        public PaginationOrderPropertyBuilder PaginationSequence<TProperty>(Expression<Func<TEntity, TProperty>> orderPropertyExpression)
        {
            return new PaginationOrderPropertyBuilder(orderPropertyExpression, _metadata);
        }
        /// <summary>
        /// 配置反向排序 仅支持单排序 当skip>= reverseTotalGe*reverseFactor使用反向排序
        /// </summary>
        /// <param name="reverseFactor"></param>
        /// <param name="reverseTotalGe"></param>
        /// <returns></returns>
        public PaginationBuilder<TEntity> ConfigReverseShardingPage(double reverseFactor=0.5,long reverseTotalGe=10000L)
        {
            _metadata.ReverseFactor = reverseFactor;
            _metadata.ReverseTotalGe = reverseTotalGe;
            return this;
        }
        ///// <summary>
        ///// 配置当分表数目小于多少后直接取到内存不在流式处理
        ///// </summary>
        ///// <param name="count"></param>
        ///// <returns></returns>
        //public PaginationBuilder<TEntity> ConfigTakeInMemoryCountIfLe(int count)
        //{
        //    _metadata.TakeInMemoryCountIfLe = count;
        //    return this;
        //}

        /// <summary>
        /// 启用多次查询排序
        /// </summary>
        /// <returns></returns>
        public PaginationBuilder<TEntity> ConfigMultiQueryShardingPage(IMultiQueryPredicate multiQueryPredicate)
        {
            _metadata.MultiQueryPredicate = multiQueryPredicate;
            return this;
        }
    }
}
