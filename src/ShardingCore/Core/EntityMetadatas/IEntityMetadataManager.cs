using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 元数据管理者 无法通过依赖注入获取,请是用泛型方法依赖注入来获取<see cref="IEntityMetadataManager&lt;TShardingDbContext&gt;"/>
    /// </summary>
    public interface IEntityMetadataManager
    {
        /// <summary>
        /// 添加元数据
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <returns></returns>
        bool AddEntityMetadata(EntityMetadata entityMetadata);
        /// <summary>
        /// 是否分表
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        bool IsShardingTable(Type entityType);
        /// <summary>
        /// 是否分库
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        bool IsShardingDataSource(Type entityType);
        /// <summary>
        /// 尝试获取
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        EntityMetadata TryGet(Type entityType);
    }
}
