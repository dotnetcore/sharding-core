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
        /// 配置分表的一些信息
        /// 1.ShardingProperty 哪个字段分表
        /// 2.TableSeparator 分表的后缀和表名的连接符
        /// 3.AutoCreateTable 启动时是否需要创建对应的分表信息
        /// 3.ShardingExtraProperty 额外分片字段
        /// </summary>
        /// <param name="builder"></param>
         void Configure(EntityMetadataTableBuilder<TEntity> builder);
    }
}
