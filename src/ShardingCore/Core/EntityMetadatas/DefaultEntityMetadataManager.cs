using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 默认分片对象元数据管理者实现
    /// </summary>
    /// <typeparam name="TShardingDbContext"></typeparam>
    public class DefaultEntityMetadataManager<TShardingDbContext> : IEntityMetadataManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ConcurrentDictionary<Type, EntityMetadata> _caches =new ();
        public bool AddEntityMetadata(EntityMetadata entityMetadata)
        {
            return _caches.TryAdd(entityMetadata.EntityType, entityMetadata);
        }
        /// <summary>
        /// 对象是否是分表对象
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public bool IsShardingTable(Type entityType)
        {
            if(!_caches.TryGetValue(entityType,out var entityMetadata))
                return false;
            return entityMetadata.IsMultiTableMapping;
        }

        public bool IsOnlyShardingTable(Type entityType)
        {
            return  IsShardingTable(entityType) && !IsShardingDataSource(entityType);
        }

        /// <summary>
        /// 对象是否是分库对象
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public bool IsShardingDataSource(Type entityType)
        {
            if (!_caches.TryGetValue(entityType, out var entityMetadata))
                return false;
            return entityMetadata.IsMultiDataSourceMapping;
        }

        public bool IsOnlyShardingDataSource(Type entityType)
        {
            return IsShardingDataSource(entityType) && !IsShardingTable(entityType);
        }

        /// <summary>
        /// 对象获取没有返回null
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public EntityMetadata TryGet(Type entityType)
        {
            if (!_caches.TryGetValue(entityType, out var entityMetadata))
                return null;
            return entityMetadata;
        }
        /// <summary>
        /// 是否是分片对象(包括分表或者分库)
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public bool IsSharding(Type entityType)
        {
            return _caches.ContainsKey(entityType);
        }
    }
}
