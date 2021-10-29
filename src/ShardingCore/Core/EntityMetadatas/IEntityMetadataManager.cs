using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.EntityMetadatas
{
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
