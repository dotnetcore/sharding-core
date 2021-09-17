using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;

namespace ShardingCore.Core.VirtualDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 06 February 2021 15:24:08
    * @Email: 326308290@qq.com
    */
    public class VirtualDataSourceManager : IVirtualDataSourceManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, IVirtualDataSource> _virtualDataSources = new ConcurrentDictionary<Type, IVirtualDataSource>();

        private readonly Dictionary<string, ISet<Type>> _shardingConnectKeys = new Dictionary<string,ISet<Type>>();
        private readonly Dictionary<Type, ISet<string>> _entityTypeConnectKeyIndex = new Dictionary<Type, ISet<string>>();

        public VirtualDataSourceManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            //var shardingEntities = AssemblyHelper.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes())
            //    .Where(type => !String.IsNullOrEmpty(type.Namespace))
            //    .Where(type => !type.IsAbstract&&type.GetInterfaces()
            //        .Any(it => it.IsInterface  &&typeof(IShardingDataSource)==it)
            //    );
            //foreach (var shardingEntity in shardingEntities)
            //{
            //    Type genericType = typeof(IVirtualDataSource<>);
            //    Type interfaceType = genericType.MakeGenericType(shardingEntity);
            //    var virtualDataSource = (IVirtualDataSource)serviceProvider.GetService(interfaceType);
            //    _virtualDataSources.TryAdd(virtualDataSource.EntityType, virtualDataSource);
            //}
        }

        public ISet<string> GetAllShardingConnectKeys()
        {
            return _shardingConnectKeys.Keys.ToHashSet();
        }


        public List<IVirtualDataSource> GetAllDataSources()
        {
            return _virtualDataSources.Select(o => o.Value).ToList();
        }

        public void AddConnectEntities(string connectKey, Type entityType)
        {
            if (!_shardingConnectKeys.ContainsKey(connectKey))
                throw new ShardingCoreException("connectKey not init");
            _shardingConnectKeys[connectKey].Add(entityType);
            BuildIndex(connectKey, entityType);
        }

        private void BuildIndex(string connectKey, Type entityType)
        {
            
            if (_entityTypeConnectKeyIndex.ContainsKey(entityType))
            {
                _entityTypeConnectKeyIndex[entityType].Add(connectKey);
            }
            else
            {
                _entityTypeConnectKeyIndex.Add(entityType,new HashSet<string>(){ connectKey
                });
            }
        }

        public IVirtualDataSource GetVirtualDataSource(Type shardingEntityType)
        {
            if (!_virtualDataSources.TryGetValue(shardingEntityType, out var virtualTable) || virtualTable == null)
                throw new VirtualDataSourceNotFoundException($"{shardingEntityType}");
            return virtualTable;
        }

        public IVirtualDataSource<T> GetVirtualDataSource<T>() where T : class, IShardingDataSource
        {
            return (IVirtualDataSource<T>)GetVirtualDataSource(typeof(T));
        }

        public List<string> GetEntityTypeLinkedConnectKeys(Type shardingEntityType)
        {
            if (!_entityTypeConnectKeyIndex.ContainsKey(shardingEntityType))
                throw new ShardingCoreException($"entity:[{shardingEntityType}] not found");
            return _entityTypeConnectKeyIndex[shardingEntityType].ToList();
        }

        public void AddShardingConnectKey(string connectKey)
        {
            if (!_shardingConnectKeys.ContainsKey(connectKey))
                _shardingConnectKeys.Add(connectKey,new HashSet<Type>());
        }

        public void AddVirtualDataSource(IVirtualDataSource virtualDataSource)
        {
            _virtualDataSources.TryAdd(virtualDataSource.EntityType, virtualDataSource);
        }
    }
}