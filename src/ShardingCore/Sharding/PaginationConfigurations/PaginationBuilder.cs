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
        /// <summary>
        /// 分页顺序
        /// </summary>
        /// <param name="orderPropertyExpression"></param>
        /// <typeparam name="TProperty"></typeparam>
        public PaginationOrderPropertyBuilder PaginationSequence<TProperty>(Expression<Func<TEntity, TProperty>> orderPropertyExpression)
        {
            return new PaginationOrderPropertyBuilder(orderPropertyExpression);
        }
    }
}
