using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core;

namespace ShardingCore.Sharding.PaginationConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/1 17:32:36
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IPaginationConfiguration<TEntity> where TEntity : class,IShardingTable
    {
        /// <summary>
        ///     Configures the entity of type <typeparamref name="TEntity" />.
        /// </summary>
        /// <param name="builder"> The builder to be used to configure the entity type. </param>
        void Configure(Paginati onBuilder<TEntity> builder);
    }
}
