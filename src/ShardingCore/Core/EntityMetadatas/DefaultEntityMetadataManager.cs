using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 默认分片对象原数据管理者实现
    /// </summary>
    /// <typeparam name="TShardingDbContext"></typeparam>
    public class DefaultEntityMetadataManager<TShardingDbContext> : IEntityMetadataManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ConcurrentDictionary<Type, EntityMetadata> _caches =
            new ConcurrentDictionary<Type, EntityMetadata>();
        public bool AddEntityMetadata(EntityMetadata entityMetadata)
        {
            return _caches.TryAdd(entityMetadata.EntityType, entityMetadata);
        }

        public bool IsShardingTable(Type entityType)
        {
            if(!_caches.TryGetValue(entityType,out var entityMetadata))
                return false;
            return entityMetadata.IsMultiTableMapping;
        }

        public bool IsShardingDataSource(Type entityType)
        {
            if (!_caches.TryGetValue(entityType, out var entityMetadata))
                return false;
            return entityMetadata.IsMultiDataSourceMapping;
        }

        public EntityMetadata TryGet(Type entityType)
        {
            if (!_caches.TryGetValue(entityType, out var entityMetadata))
                return null;
            return entityMetadata;
        }
    }
}
