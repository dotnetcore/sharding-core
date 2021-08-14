using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Exceptions;

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
    public class OneDbVirtualTableManager : IVirtualTableManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, IVirtualTable> _shardingVirtualTables = new ConcurrentDictionary<Type, IVirtualTable>();
        private readonly ConcurrentDictionary<string, IVirtualTable> _shardingOriginalTaleVirtualTales = new ConcurrentDictionary<string, IVirtualTable>();
        public OneDbVirtualTableManager(IServiceProvider serviceProvider)
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

        public void AddVirtualTable(IVirtualTable virtualTable)
        {
            if (!_shardingVirtualTables.ContainsKey(virtualTable.EntityType))
            {
                _shardingVirtualTables.TryAdd(virtualTable.EntityType, virtualTable);
            }

            if (!_shardingOriginalTaleVirtualTales.ContainsKey(virtualTable.GetOriginalTableName()))
            {
                _shardingOriginalTaleVirtualTales.TryAdd(virtualTable.GetOriginalTableName(), virtualTable);
            }
        }

        public IVirtualTable GetVirtualTable(Type shardingEntityType)
        {
            if (!_shardingVirtualTables.TryGetValue(shardingEntityType, out var virtualTable))
                throw new ShardingVirtualTableNotFoundException(shardingEntityType.FullName);
            return virtualTable;
        }


        public IVirtualTable<T> GetVirtualTable<T>() where T : class, IShardingTable
        {
            return (IVirtualTable<T>)GetVirtualTable(typeof(T));
        }

        public IVirtualTable GetVirtualTable(string originalTableName)
        {
            if (!_shardingOriginalTaleVirtualTales.TryGetValue(originalTableName, out var virtualTable)||virtualTable==null)
                throw new ShardingVirtualTableNotFoundException(originalTableName);
            return virtualTable;
        }

        public IVirtualTable TryGetVirtualTable(string originalTableName)
        {
            if (!_shardingOriginalTaleVirtualTales.TryGetValue(originalTableName, out var virtualTable))
                return null;
            return virtualTable;
        }

        public List<IVirtualTable> GetAllVirtualTables()
        {
            return _shardingVirtualTables.Values.ToList();
        }

        public void AddPhysicTable(IVirtualTable virtualTable, IPhysicTable physicTable)
        {
            AddPhysicTable(virtualTable.EntityType, physicTable);
        }

        public void AddPhysicTable(Type shardingEntityType, IPhysicTable physicTable)
        {
            var virtualTable = GetVirtualTable(shardingEntityType);
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
        //    return _virtualTables.TryGetValue(shardingEntityType, out var virtualTable) && virtualTable.ShardingConfig.ShardingField == shardingField;
        //}
    }
}