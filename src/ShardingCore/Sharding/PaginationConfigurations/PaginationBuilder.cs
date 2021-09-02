using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core;

namespace ShardingCore.Sharding.PaginationConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/1 17:33:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class PaginationBuilder<TEntity> where TEntity:class,IShardingTable
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
        /// 配置当跳过多少条后开始启用只能分页
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        public PaginationBuilder<TEntity> ConfigUseShardingPageIfGeSkip(long skip)
        {
            _metadata.UseShardingPageIfGeSkipAvg = skip;
            return this;
        }
        /// <summary>
        /// 配置当分表数目小于多少后直接取到内存不在流式处理
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public PaginationBuilder<TEntity> ConfigTakeInMemoryCountIfLe(int count)
        {
            _metadata.TakeInMemoryCountIfLe = count;
            return this;
        }
    }
}
