//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.EntityFrameworkCore;
//using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
//using ShardingCore.Core.VirtualRoutes;
//using ShardingCore.Sharding.Abstractions;

//namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: Saturday, 06 February 2021 15:24:08
//    * @Email: 326308290@qq.com
//    */
//    public class VirtualDataSourceManager<TShardingDbContext> : IVirtualDataSourceManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
//    {
//        private readonly IServiceProvider _serviceProvider;
//        /// <summary>
//        /// {sharding db context type :{entity type:virtual data source}}
//        /// </summary>
//        private readonly ConcurrentDictionary<Type, IVirtualDataSource> _virtualDataSources = new ConcurrentDictionary<Type, IVirtualDataSource>();



//        public VirtualDataSourceManager(IServiceProvider serviceProvider)
//        {
//            _serviceProvider = serviceProvider;
//            //var shardingEntities = AssemblyHelper.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes())
//            //    .Where(type => !String.IsNullOrEmpty(type.Namespace))
//            //    .Where(type => !type.IsAbstract&&type.GetInterfaces()
//            //        .Any(it => it.IsInterface  &&typeof(IShardingDataSource)==it)
//            //    );
//            //foreach (var shardingEntity in shardingEntities)
//            //{
//            //    Type genericType = typeof(IVirtualDataSource<>);
//            //    Type interfaceType = genericType.MakeGenericType(shardingEntity);
//            //    var virtualDataSource = (IVirtualDataSource)serviceProvider.GetService(interfaceType);
//            //    _virtualDataSources.TryAdd(virtualDataSource.EntityType, virtualDataSource);
//            //}
//        }

//        public bool AddPhysicDataSource(IPhysicDataSource physicDataSource)
//        {
//            throw new NotImplementedException();
//        }

//        public IVirtualDataSource GetVirtualDataSource()
//        {
//            if (!_virtualDataSources.TryGetValue(shardingDbContextType, out var virtualDataSource))
//                throw new ShardingCoreInvalidOperationException($"not found virtual data source sharding db context type:[{shardingDbContextType.FullName}]");
//            return virtualDataSource;
//        }

//        public IPhysicDataSource GetDefaultDataSource()
//        {
//            var virtualDataSource = GetVirtualDataSource(shardingDbContextType);
//            return virtualDataSource.GetDefaultDataSource();
//        }

//        public string GetDefaultDataSourceName()
//        {
//            var virtualDataSource = GetVirtualDataSource(shardingDbContextType);
//            return virtualDataSource.DefaultDataSourceName;
//        }

//        public IPhysicDataSource GetPhysicDataSource(string dataSourceName)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}