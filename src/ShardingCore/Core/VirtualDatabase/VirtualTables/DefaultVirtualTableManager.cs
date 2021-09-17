using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualTables
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 18 December 2020 14:52:42
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 同一个数据库下的虚拟表管理者
    /// </summary>
    public class DefaultVirtualTableManager : IVirtualTableManager
    {
        private readonly IServiceProvider _serviceProvider;
        //{sharidngDbContextType:{entityType,virtualTableType}}
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, IVirtualTable>> _shardingVirtualTables = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, IVirtualTable>>();
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, IVirtualTable>> _shardingOriginalTaleVirtualTales = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IVirtualTable>>();
        public DefaultVirtualTableManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            //var shardingEntities = AssemblyHelper.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes())
            //    .Where(type => !String.IsNullOrEmpty(type.Namespace))
            //    .Where(type => !type.IsAbstract&&type.GetInterfaces()
            //        .Any(it => it.IsInterface  &&typeof(IShardingTable)==it)
            //    );
            //foreach (var shardingEntity in shardingEntities)
            //{
            //    Type genericType = typeof(IVirtualTable<>);
            //    Type interfaceType = genericType.MakeGenericType(shardingEntity);
            //    var virtualTable = (IVirtualTable)serviceProvider.GetService(interfaceType);
            //    _virtualTables.TryAdd(virtualTable.EntityType, virtualTable);
            //}
        }

        private void CheckShardingDbContextType(Type shardingDbContextType, string dsname)
        {
            if (!shardingDbContextType.IsShardingDbContext())
                throw new ShardingCoreException(
                    $"{shardingDbContextType.FullName} must impl {nameof(IShardingDbContext)}");
        }

        private void CheckShardingTableEntityType(string dsname, Type shardingEntityType)
        {
            if (!shardingEntityType.IsShardingTable())
                throw new ShardingCoreException(
                    $"{shardingEntityType.FullName} must impl {nameof(IShardingTable)}");
        }
        private string CreateShardingEntityTypeKey(Type shardingDbContextType, string dsname, Type entityType)
        {
            return $"{shardingDbContextType.FullName}{entityType.FullName}";
        }
        private string CreateShardingTableNameKey(Type shardingDbContextType, string dsname, string originalTableName)
        {
            return $"{shardingDbContextType.FullName}{originalTableName}";
        }

        public void AddVirtualTable(Type shardingDbContextType, string dsname, IVirtualTable virtualTable)
        {
            CheckShardingDbContextType(shardingDbContextType, dsname);

            var innerShardingVirtualTables = _shardingVirtualTables.GetOrAdd(shardingDbContextType,
                key => new ConcurrentDictionary<Type, IVirtualTable>());

            if (!innerShardingVirtualTables.ContainsKey(virtualTable.EntityType))
            {
                innerShardingVirtualTables.TryAdd(virtualTable.EntityType, virtualTable);
            }

            var innerShardingOriginalTableVirtualTables = _shardingOriginalTaleVirtualTales.GetOrAdd(shardingDbContextType,type=>new ConcurrentDictionary<string, IVirtualTable>());

            if (!innerShardingOriginalTableVirtualTables.ContainsKey(virtualTable.GetOriginalTableName()))
            {
                innerShardingOriginalTableVirtualTables.TryAdd(virtualTable.GetOriginalTableName(), virtualTable);
            }
        }

        public IVirtualTable GetVirtualTable(Type shardingDbContextType, string dsname, Type shardingEntityType)
        {
            CheckShardingDbContextType(shardingDbContextType,dsname);
            CheckShardingTableEntityType(dsname,shardingEntityType);

            var shardingKey = CreateShardingEntityTypeKey(shardingDbContextType, dsname, shardingEntityType);
            if(!_shardingVirtualTables.TryGetValue(shardingDbContextType,out var innerShardingVirtualTables) || innerShardingVirtualTables.IsEmpty())
                throw new ShardingVirtualTableNotFoundException(shardingDbContextType.FullName);

            if (!innerShardingVirtualTables.TryGetValue(shardingEntityType, out var virtualTable)||virtualTable==null)
                throw new ShardingVirtualTableNotFoundException(shardingEntityType.FullName);
            return virtualTable;
        }


        public IVirtualTable<T> GetVirtualTable<TDbContext, T>(string dsname) where T : class, IShardingTable where TDbContext : DbContext, IShardingDbContext
        {
            return (IVirtualTable<T>)GetVirtualTable(typeof(TDbContext), dsname, typeof(T));
        }

        public IVirtualTable GetVirtualTable(Type shardingDbContextType, string dsname, string originalTableName)
        {
            CheckShardingDbContextType(shardingDbContextType, dsname);
            if (!_shardingOriginalTaleVirtualTales.TryGetValue(shardingDbContextType, out var innerShardingOriginalTableVirtualTables) || innerShardingOriginalTableVirtualTables.IsEmpty())
                throw new ShardingVirtualTableNotFoundException(shardingDbContextType.FullName);
            if(!innerShardingOriginalTableVirtualTables.TryGetValue(originalTableName,out var virtualTable)|| virtualTable==null)
                throw new ShardingVirtualTableNotFoundException(originalTableName);
            return virtualTable;
        }

        public IVirtualTable GetVirtualTable<TDbContext>(string dsname, string originalTableName) where TDbContext : DbContext, IShardingDbContext
        {
            return GetVirtualTable(typeof(TDbContext), dsname,originalTableName);
        }

        public IVirtualTable TryGetVirtualTable(Type shardingDbContextType, string dsname, string originalTableName)
        {
            CheckShardingDbContextType(shardingDbContextType,dsname);
            if (!_shardingOriginalTaleVirtualTales.TryGetValue(shardingDbContextType,
                out var innerShardingOriginalTableVirtualTables) || innerShardingOriginalTableVirtualTables.IsEmpty())
                return null;
            if (!innerShardingOriginalTableVirtualTables.TryGetValue(originalTableName, out var virtualTable) || virtualTable == null)
                return null;
            return virtualTable;
        }

        public IVirtualTable TryGetVirtualTablee<TDbContext>(string dsname, string originalTableName) where TDbContext : DbContext, IShardingDbContext
        {
            return TryGetVirtualTable(typeof(TDbContext), dsname, originalTableName);
        }


        public List<IVirtualTable> GetAllVirtualTables(Type shardingDbContextType, string dsname)
        {
            if (!_shardingOriginalTaleVirtualTales.TryGetValue(shardingDbContextType,
                out var innerShardingOriginalTableVirtualTables) || innerShardingOriginalTableVirtualTables.IsEmpty())
                return new List<IVirtualTable>();
            var keyPrefix = shardingDbContextType.FullName;
            return innerShardingOriginalTableVirtualTables.Values.ToList();
        }

        public List<IVirtualTable> GetAllVirtualTables<TDbContext>(string dsname) where TDbContext : DbContext, IShardingDbContext
        {
            return GetAllVirtualTables(typeof(TDbContext),dsname);
        }

        public void AddPhysicTable(Type shardingDbContextType, string dsname, IVirtualTable virtualTable, IPhysicTable physicTable)
        {
            AddPhysicTable(shardingDbContextType, dsname, virtualTable.EntityType, physicTable);
        }

        public void AddPhysicTable<TDbContext>(string dsname, IVirtualTable virtualTable, IPhysicTable physicTable) where TDbContext : DbContext, IShardingDbContext
        {
            AddPhysicTable(typeof(TDbContext), dsname, virtualTable.EntityType, physicTable);
        }


        public void AddPhysicTable(Type shardingDbContextType, string dsname, Type shardingEntityType, IPhysicTable physicTable)
        {
            var virtualTable = GetVirtualTable(shardingDbContextType, dsname,shardingEntityType);
            virtualTable.AddPhysicTable(physicTable);
        }


        public void AddPhysicTable<TDbContext>(string dsname, Type shardingEntityType, IPhysicTable physicTable) where TDbContext : DbContext, IShardingDbContext
        {
            var virtualTable = GetVirtualTable(typeof(TDbContext), dsname,shardingEntityType);
            virtualTable.AddPhysicTable(physicTable);
        }

        ///// <summary>
        ///// 是否是分表字段
        ///// </summary>
        ///// <param name="shardingEntityType"></param>
        ///// <param name="shardingField"></param>
        ///// <returns></returns>
        //public bool IsShardingKey(Type shardingEntityType, string shardingField)
        //{
        //    return _virtualTables.TryGetValue(shardingEntityType, out var virtualTable) && virtualTable.ShardingConfigOption.ShardingField == shardingField;
        //}
    }
}