using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Helpers;

namespace ShardingCore.Core.VirtualDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 06 February 2021 15:24:08
* @Email: 326308290@qq.com
*/
    public class VirtualDataSourceManager:IVirtualDataSourceManager
    {
        private readonly ConcurrentDictionary<Type, IVirtualDataSource> _virtualDataSources = new ConcurrentDictionary<Type, IVirtualDataSource>();

        public VirtualDataSourceManager(IServiceProvider serviceProvider)
        {
            var shardingEntities = AssemblyHelper.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes())
                .Where(type => !String.IsNullOrEmpty(type.Namespace))
                .Where(type => !type.IsAbstract&&type.GetInterfaces()
                    .Any(it => it.IsInterface  &&typeof(IShardingDataSource)==it)
                );
            foreach (var shardingEntity in shardingEntities)
            {
                Type genericType = typeof(IVirtualDataSource<>);
                Type interfaceType = genericType.MakeGenericType(shardingEntity);
                var virtualDataSource = (IVirtualDataSource)serviceProvider.GetService(interfaceType);
                _virtualDataSources.TryAdd(virtualDataSource.EntityType, virtualDataSource);
            }
        }
        public List<IVirtualDataSource> GetAllVirtualDataSources()
        {
            return _virtualDataSources.Select(o => o.Value).ToList();
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

        public void AddVirtualDataSource(IVirtualDataSource virtualDataSource)
        {
            _virtualDataSources.TryAdd(virtualDataSource.EntityType, virtualDataSource);
        }

        public void AddPhysicDataSource(IVirtualDataSource virtualDataSource, IPhysicDataSource physicDataSource)
        {
            AddPhysicDataSource(virtualDataSource.EntityType, physicDataSource);
        }

        public void AddPhysicDataSource(Type shardingEntityType, IPhysicDataSource physicDataSource)
        {
            var virtualTable = GetVirtualDataSource(shardingEntityType);
            virtualTable.AddDataSource(physicDataSource);
        }

    }
}