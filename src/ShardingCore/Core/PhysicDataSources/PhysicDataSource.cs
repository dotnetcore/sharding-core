using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.Internal;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.Core.PhysicDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 13:47:49
* @Email: 326308290@qq.com
*/
    public class PhysicDataSource:IPhysicDataSource
    {
        public string ConnectionString { get; }
        private readonly DataSourceEnum _dataSourceType;
        private readonly Dictionary<Type,ShardingEntityBaseType> _entities;
        private readonly string _connectionString;

        public PhysicDataSource(string connectionString,DataSourceEnum dataSourceType)
        {
            _connectionString = connectionString;
            _dataSourceType = dataSourceType;
            _entities = new Dictionary<Type, ShardingEntityBaseType>();
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public DataSourceEnum GetDataSourceType()
        {
            return _dataSourceType;
        }

        public void AddEntity(ShardingEntityBaseType entityBaseType)
        {
            if(!_entities.ContainsKey(entityBaseType.EntityType))
                _entities.Add(entityBaseType.EntityType,entityBaseType);
        }

        public void AddEntity<T>() where T : class, IShardingDataSource
        {
            var entityType = typeof(T);
            AddEntity(entityType);
        }

        public void AddEntity(Type shardingEntity)
        {
            if (!shardingEntity.IsShardingDataSource())
                throw new InvalidOperationException($"entity:[{shardingEntity}] should from {nameof(IShardingDataSource)}");
            var entityBaseType = ShardingUtil.Parse(shardingEntity);
            AddEntity(entityBaseType);
        } 

        public bool HasEntity(Type shardingEntity)
        {
            if (!shardingEntity.IsShardingDataSource())
                return false;
            return _entities.ContainsKey(shardingEntity);
        }

        public bool HasEntity<T>() where T : class, IShardingDataSource
        {
            return HasEntity(typeof(T));
        }


    }
}