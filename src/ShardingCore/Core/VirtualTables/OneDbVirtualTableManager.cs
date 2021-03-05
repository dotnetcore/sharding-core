using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Exceptions;
using ShardingCore.Helpers;

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
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, IVirtualTable>> _shardingVirtualTables = new ConcurrentDictionary<string, ConcurrentDictionary<Type, IVirtualTable>>();

        public OneDbVirtualTableManager(IServiceProvider serviceProvider)
        {
            //var shardingEntities = AssemblyHelper.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes())
            //    .Where(type => !String.IsNullOrEmpty(type.Namespace))
            //    .Where(type => !type.IsAbstract&&type.GetInterfaces()
            //        .Any(it => it.IsInterface  &&typeof(IShardingEntity)==it)
            //    );
            //foreach (var shardingEntity in shardingEntities)
            //{
            //    Type genericType = typeof(IVirtualTable<>);
            //    Type interfaceType = genericType.MakeGenericType(shardingEntity);
            //    var virtualTable = (IVirtualTable)serviceProvider.GetService(interfaceType);
            //    _virtualTables.TryAdd(virtualTable.EntityType, virtualTable);
            //}
        }

        public void AddVirtualTable(string connectKey, IVirtualTable virtualTable)
        {
            if (!_shardingVirtualTables.ContainsKey(connectKey))
            {
                _shardingVirtualTables.TryAdd(connectKey, new ConcurrentDictionary<Type, IVirtualTable>());
            }

            var shardingVirtualTable = _shardingVirtualTables[connectKey];
            shardingVirtualTable.TryAdd(virtualTable.EntityType, virtualTable);
        }

        public IVirtualTable GetVirtualTable(string connectKey, Type shardingEntityType)
        {
            if (!_shardingVirtualTables.TryGetValue(connectKey, out var tableKv))
                throw new ShardingVirtualTableNotFoundException(connectKey);
            if (!tableKv.TryGetValue(shardingEntityType, out var virtualTable))
                throw new ShardingVirtualTableNotFoundException($"[{connectKey}]-[{shardingEntityType}]");
            return virtualTable;
        }

        public IVirtualTable<T> GetVirtualTable<T>(string connectKey) where T : class, IShardingEntity
        {
            return (IVirtualTable<T>)GetVirtualTable(connectKey, typeof(T));
        }

        public IVirtualTable GetVirtualTable(string connectKey, string originalTableName)
        {
            if (!_shardingVirtualTables.TryGetValue(connectKey, out var tableKv))
                throw new ShardingVirtualTableNotFoundException(connectKey);
            var virtualTable = tableKv.Where(o => o.Value.GetOriginalTableName() == originalTableName).Select(o=>o.Value).FirstOrDefault();
            if (virtualTable==null)
                throw new ShardingVirtualTableNotFoundException($"[{connectKey}]-[{originalTableName}]");
            return virtualTable;
        }

        public List<IVirtualTable> GetAllVirtualTables(string connectKey)
        {
            if (!_shardingVirtualTables.ContainsKey(connectKey))
                return new List<IVirtualTable>(0);
            return _shardingVirtualTables[connectKey].Select(o => o.Value).ToList();
        }

        public void AddPhysicTable(string connectKey, IVirtualTable virtualTable, IPhysicTable physicTable)
        {
            AddPhysicTable(connectKey, virtualTable.EntityType, physicTable);
        }

        public void AddPhysicTable(string connectKey, Type shardingEntityType, IPhysicTable physicTable)
        {
            if (!_shardingVirtualTables.ContainsKey(connectKey))
                throw new ShardingConnectKeyNotFoundException(connectKey);
            var virtualTable = GetVirtualTable(connectKey,shardingEntityType);
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