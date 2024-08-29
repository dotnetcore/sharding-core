﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 默认分片对象元数据管理者实现
    /// </summary>
    public class DefaultEntityMetadataManager : IEntityMetadataManager
    {
        private readonly ShardingConfigOptions _shardingConfigOptions;
        private readonly ConcurrentDictionary<Type, EntityMetadata> _caches = new();

        private readonly ConcurrentDictionary<string /*logic table name*/, List<EntityMetadata>> _logicTableCaches =
            new();

        public DefaultEntityMetadataManager(ShardingConfigOptions shardingConfigOptions)
        {
            _shardingConfigOptions = shardingConfigOptions;
        }

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
            if (!_caches.TryGetValue(entityType, out var entityMetadata))
                return false;
            return entityMetadata.IsMultiTableMapping;
        }

        public bool IsOnlyShardingTable(Type entityType)
        {
            if (!_caches.TryGetValue(entityType, out var entityMetadata))
                return false;
            return entityMetadata.IsMultiTableMapping && !entityMetadata.IsMultiDataSourceMapping;
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
            if (!_caches.TryGetValue(entityType, out var entityMetadata))
                return false;
            return entityMetadata.IsMultiDataSourceMapping && !entityMetadata.IsMultiTableMapping;
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

        public List<EntityMetadata> TryGetByLogicTableName(string logicTableName)
        {
            if (_logicTableCaches.TryGetValue(logicTableName, out var metadata))
            {
                return metadata;
            }

            return null;
        }

        /// <summary>
        /// 是否是分片对象(包括分表或者分库)
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public bool IsSharding(Type entityType)
        {
            if (!_caches.TryGetValue(entityType, out var metadata))
            {
                return false;
            }

            return metadata.IsShardingTable() || metadata.IsShardingDataSource();
        }

        public List<Type> GetAllShardingEntities()
        {
            return _caches.Where(o => o.Value.IsShardingTable() || o.Value.IsShardingDataSource()).Select(o => o.Key)
                .ToList();
        }

        public bool TryInitModel(IEntityType efEntityType)
        {
            if (_caches.TryGetValue(efEntityType.ClrType, out var metadata))
            {
                if (_shardingConfigOptions.CheckShardingKeyValueGenerated)
                {
                    var propertiesMap = efEntityType.GetProperties().ToDictionary(o=>o.Name,o=>o);
                    if (metadata.IsMultiDataSourceMapping)
                    {
                        foreach (var metadataProperty in metadata.ShardingDataSourceProperties)
                        {
                            var propertyName = metadataProperty.Key;
                            if (propertiesMap.TryGetValue(propertyName, out var property))
                            {
                                if (property.ValueGenerated != ValueGenerated.Never)
                                {
                                    throw new ShardingCoreConfigException(
                                        $"sharding data source key:{propertyName} is not {nameof(ValueGenerated)}.{nameof(ValueGenerated.Never)}");
                                }
                            }
                        }
                    }
                    if (metadata.IsMultiTableMapping)
                    {
                        foreach (var metadataProperty in metadata.ShardingTableProperties)
                        {
                            var propertyName = metadataProperty.Key;
                            if (propertiesMap.TryGetValue(propertyName, out var property))
                            {
                                if (property.ValueGenerated != ValueGenerated.Never)
                                {
                                    throw new ShardingCoreConfigException(
                                        $"sharding table key:{propertyName} is not {nameof(ValueGenerated)}.{nameof(ValueGenerated.Never)}");
                                }
                            }
                        }
                    }
                }

                metadata.SetEntityModel(efEntityType);

                if (string.IsNullOrWhiteSpace(metadata.LogicTableName))
                {
                    throw new ShardingCoreInvalidOperationException(
                        $"init model error, cant get logic table name:[{metadata.LogicTableName}] from  entity:[{efEntityType.ClrType}],is view:[{metadata.IsView}]");
                }

                if (!_logicTableCaches.TryGetValue(metadata.LogicTableName, out var metadatas))
                {
                    metadatas = new List<EntityMetadata>();
                    _logicTableCaches.TryAdd(metadata.LogicTableName, metadatas);
                }

                if (metadatas.All(o => o.EntityType != efEntityType.ClrType))
                {
                    metadatas.Add(metadata);
                    return true;
                }

                //添加完成后检查逻辑表对应的对象不可以存在两个以上的分片
                if (metadatas.Count > 1 && metadatas.Any(o => o.IsShardingTable() || o.IsShardingDataSource()))
                {
                    throw new ShardingCoreInvalidOperationException(
                        $"cant add logic table name caches for metadata:[{metadata.LogicTableName}-{efEntityType.ClrType}]");
                }
            }

            return false;
        }
    }
}