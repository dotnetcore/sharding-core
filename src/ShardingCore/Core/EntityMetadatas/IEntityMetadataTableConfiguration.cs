using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.EntityShardingMetadatas;

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 元数据分表对象配置 由具体对应的分表路由实现配置
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntityMetadataTableConfiguration<TEntity>where TEntity : class
    {
        /// <summary>
        /// 配置对象的分表信息
        /// </summary>
        /// <param name="builder"></param>
         void Configure(EntityMetadataTableBuilder<TEntity> builder);
    }
}
