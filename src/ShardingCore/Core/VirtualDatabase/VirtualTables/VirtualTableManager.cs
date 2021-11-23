using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualTables
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
    public class VirtualTableManager<TShardingDbContext> : IVirtualTableManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;

        /// <summary>
        /// {entityType,virtualTableType}
        /// </summary>
        private readonly ConcurrentDictionary<Type, IVirtualTable> _shardingVirtualTables = new ConcurrentDictionary<Type, IVirtualTable>();
        private readonly ConcurrentDictionary<string, IVirtualTable> _shardingVirtualTaleVirtualTables = new ConcurrentDictionary<string, IVirtualTable>();
        public VirtualTableManager(IEntityMetadataManager<TShardingDbContext> entityMetadataManager)
        {
            _entityMetadataManager = entityMetadataManager;
        }

        public bool AddVirtualTable(IVirtualTable virtualTable)
        {
            var result = _shardingVirtualTables.TryAdd(virtualTable.EntityMetadata.EntityType, virtualTable);
            _shardingVirtualTaleVirtualTables.TryAdd(virtualTable.GetVirtualTableName(), virtualTable);
            return result;
        }
        /// <summary>
        /// 获取对应的虚拟表
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <returns></returns>
        public IVirtualTable GetVirtualTable(Type shardingEntityType)
        {
            if (!_entityMetadataManager.IsShardingTable(shardingEntityType))
                throw new ShardingCoreInvalidOperationException(shardingEntityType.FullName);
            if (!_shardingVirtualTables.TryGetValue(shardingEntityType, out var virtualTable))
                throw new ShardingCoreException($"virtual table not found entity: {shardingEntityType.FullName}");
            return virtualTable;
        }

        public IVirtualTable TryGetVirtualTable(Type shardingEntityType)
        {
            if (!_entityMetadataManager.IsShardingTable(shardingEntityType))
                throw new ShardingCoreInvalidOperationException(shardingEntityType.FullName);
            if (!_shardingVirtualTables.TryGetValue(shardingEntityType, out var virtualTable))
                return null;
            return virtualTable;
        }

        public IVirtualTable GetVirtualTable(string virtualTableName)
        {
            if (!_shardingVirtualTaleVirtualTables.TryGetValue(virtualTableName, out var virtualTable))
                throw new ShardingCoreException($"virtual table not found virtual table name: {virtualTableName}");
            return virtualTable;
        }

        public IVirtualTable TryGetVirtualTable(string virtualTableName)
        {
            if (!_shardingVirtualTaleVirtualTables.TryGetValue(virtualTableName, out var virtualTable))
                return null;
            return virtualTable;
        }

        public ISet<IVirtualTable> GetAllVirtualTables()
        {
            return _shardingVirtualTables.Select(o => o.Value).ToHashSet();
        }

        public bool AddPhysicTable(IVirtualTable virtualTable, IPhysicTable physicTable)
        {
            return AddPhysicTable(virtualTable.EntityMetadata.EntityType, physicTable);
        }

        public bool AddPhysicTable(Type shardingEntityType, IPhysicTable physicTable)
        {
            if (!_shardingVirtualTables.TryGetValue(shardingEntityType, out var virtualTable))
                throw new ShardingCoreException($"virtual table not found entity: {shardingEntityType.FullName}");
            return virtualTable.AddPhysicTable(physicTable);
        }




        ///// <summary>
        ///// {sharidngDbContextType:{entityType,virtualTableType}}
        ///// </summary>
        //private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, IVirtualTable>> _shardingVirtualTables = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, IVirtualTable>>();
        //private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, IVirtualTable>> _shardingVirtualTaleVirtualTables = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IVirtualTable>>();
        //public VirtualTableManager()
        //{

        //}

        //private void CheckShardingDbContextType(Type shardingDbContextType)
        //{
        //    if (!shardingDbContextType.IsShardingDbContext())
        //        throw new ShardingCoreException(
        //            $"{shardingDbContextType.FullName} must impl {nameof(IShardingDbContext)}");
        //}

        //private void CheckShardingTableEntityType(Type shardingEntityType)
        //{
        //    if (!shardingEntityType.IsShardingTable())
        //        throw new ShardingCoreException(
        //            $"{shardingEntityType.FullName} must impl {nameof(IShardingTable)}");
        //}
        //private string CreateShardingEntityTypeKey(Type shardingDbContextType,Type entityType)
        //{
        //    return $"{shardingDbContextType.FullName}{entityType.FullName}";
        //}
        //private string CreateShardingTableNameKey(Type shardingDbContextType,string originalTableName)
        //{
        //    return $"{shardingDbContextType.FullName}{originalTableName}";
        //}

        //public void AddVirtualTable(Type shardingDbContextType,IVirtualTable virtualTable)
        //{
        //    CheckShardingDbContextType(shardingDbContextType);

        //    var innerShardingVirtualTables = _shardingVirtualTables.GetOrAdd(shardingDbContextType,
        //        key => new ConcurrentDictionary<Type, IVirtualTable>());

        //    if (!innerShardingVirtualTables.ContainsKey(virtualTable.EntityType))
        //    {
        //        innerShardingVirtualTables.TryAdd(virtualTable.EntityType, virtualTable);
        //    }

        //    var innerShardingOriginalTableVirtualTables = _shardingVirtualTaleVirtualTables.GetOrAdd(shardingDbContextType,type=>new ConcurrentDictionary<string, IVirtualTable>());

        //    if (!innerShardingOriginalTableVirtualTables.ContainsKey(virtualTable.GetVirtualTableName()))
        //    {
        //        innerShardingOriginalTableVirtualTables.TryAdd(virtualTable.GetVirtualTableName(), virtualTable);
        //    }
        //}

        //public IVirtualTable GetVirtualTable(Type shardingDbContextType,Type shardingEntityType)
        //{
        //    CheckShardingDbContextType(shardingDbContextType);
        //    CheckShardingTableEntityType(shardingEntityType);

        //    var shardingKey = CreateShardingEntityTypeKey(shardingDbContextType, shardingEntityType);
        //    if(!_shardingVirtualTables.TryGetValue(shardingDbContextType,out var innerShardingVirtualTables) || innerShardingVirtualTables.IsEmpty())
        //        throw new ShardingVirtualTableNotFoundException(shardingDbContextType.FullName);

        //    if (!innerShardingVirtualTables.TryGetValue(shardingEntityType, out var virtualTable)||virtualTable==null)
        //        throw new ShardingVirtualTableNotFoundException(shardingEntityType.FullName);
        //    return virtualTable;
        //}


        //public IVirtualTable<T> GetVirtualTable<TDbContext, T>() where T : class, IShardingTable where TDbContext : DbContext, IShardingDbContext
        //{
        //    return (IVirtualTable<T>)GetVirtualTable(typeof(TDbContext), typeof(T));
        //}

        //public IVirtualTable GetVirtualTable(Type shardingDbContextType, string originalTableName)
        //{
        //    CheckShardingDbContextType(shardingDbContextType);
        //    if (!_shardingVirtualTaleVirtualTables.TryGetValue(shardingDbContextType, out var innerShardingOriginalTableVirtualTables) || innerShardingOriginalTableVirtualTables.IsEmpty())
        //        throw new ShardingVirtualTableNotFoundException(shardingDbContextType.FullName);
        //    if(!innerShardingOriginalTableVirtualTables.TryGetValue(originalTableName,out var virtualTable)|| virtualTable==null)
        //        throw new ShardingVirtualTableNotFoundException(originalTableName);
        //    return virtualTable;
        //}

        //public IVirtualTable GetVirtualTable<TDbContext>(string originalTableName) where TDbContext : DbContext, IShardingDbContext
        //{
        //    return GetVirtualTable(typeof(TDbContext),originalTableName);
        //}

        //public IVirtualTable TryGetVirtualTable(Type shardingDbContextType,string originalTableName)
        //{
        //    CheckShardingDbContextType(shardingDbContextType);
        //    if (!_shardingVirtualTaleVirtualTables.TryGetValue(shardingDbContextType,
        //        out var innerShardingOriginalTableVirtualTables) || innerShardingOriginalTableVirtualTables.IsEmpty())
        //        return null;
        //    if (!innerShardingOriginalTableVirtualTables.TryGetValue(originalTableName, out var virtualTable) || virtualTable == null)
        //        return null;
        //    return virtualTable;
        //}

        //public IVirtualTable TryGetVirtualTablee<TDbContext>(string originalTableName) where TDbContext : DbContext, IShardingDbContext
        //{
        //    return TryGetVirtualTable(typeof(TDbContext), originalTableName);
        //}


        //public List<IVirtualTable> GetAllVirtualTables(Type shardingDbContextType)
        //{
        //    if (!_shardingVirtualTaleVirtualTables.TryGetValue(shardingDbContextType,
        //        out var innerShardingOriginalTableVirtualTables) || innerShardingOriginalTableVirtualTables.IsEmpty())
        //        return new List<IVirtualTable>();
        //    var keyPrefix = shardingDbContextType.FullName;
        //    return innerShardingOriginalTableVirtualTables.Values.ToList();
        //}

        //public List<IVirtualTable> GetAllVirtualTables<TDbContext>() where TDbContext : DbContext, IShardingDbContext
        //{
        //    return GetAllVirtualTables(typeof(TDbContext));
        //}

        //public void AddPhysicTable(Type shardingDbContextType,IVirtualTable virtualTable, IPhysicTable physicTable)
        //{
        //    AddPhysicTable(shardingDbContextType, virtualTable.EntityType, physicTable);
        //}

        //public void AddPhysicTable<TDbContext>(IVirtualTable virtualTable, IPhysicTable physicTable) where TDbContext : DbContext, IShardingDbContext
        //{
        //    AddPhysicTable(typeof(TDbContext),virtualTable.EntityType, physicTable);
        //}


        //public void AddPhysicTable(Type shardingDbContextType,Type shardingEntityType, IPhysicTable physicTable)
        //{
        //    var virtualTable = GetVirtualTable(shardingDbContextType,shardingEntityType);
        //    virtualTable.AddPhysicTable(physicTable);
        //}


        //public void AddPhysicTable<TDbContext>(Type shardingEntityType, IPhysicTable physicTable) where TDbContext : DbContext, IShardingDbContext
        //{
        //    var virtualTable = GetVirtualTable(typeof(TDbContext),shardingEntityType);
        //    virtualTable.AddPhysicTable(physicTable);
        //}
    }
}