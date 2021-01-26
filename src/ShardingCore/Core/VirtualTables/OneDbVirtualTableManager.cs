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
        private readonly ConcurrentDictionary<Type, IVirtualTable> _virtualTables = new ConcurrentDictionary<Type, IVirtualTable>();

        public OneDbVirtualTableManager(IServiceProvider serviceProvider)
        {
            var shardingEntities = AssemblyHelper.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes())
                .Where(type => !String.IsNullOrEmpty(type.Namespace))
                .Where(type => !type.IsAbstract&&type.GetInterfaces()
                    .Any(it => it.IsInterface  &&typeof(IShardingEntity)==it)
                );
            foreach (var shardingEntity in shardingEntities)
            {
                Type genericType = typeof(IVirtualTable<>);
                Type interfaceType = genericType.MakeGenericType(shardingEntity);
                var virtualTable = (IVirtualTable)serviceProvider.GetService(interfaceType);
                _virtualTables.TryAdd(virtualTable.EntityType, virtualTable);
            }
        }

        public List<IVirtualTable> GetAllVirtualTables()
        {
            return _virtualTables.Select(o=>o.Value).ToList();
        }

        /// <summary>
        /// 获取指定类型的虚拟表
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <returns></returns>
        /// <exception cref="VirtualTableNotFoundException"></exception>
        public IVirtualTable GetVirtualTable(Type shardingEntityType)
        {
            if (!_virtualTables.TryGetValue(shardingEntityType, out var virtualTable) || virtualTable == null)
                throw new VirtualTableNotFoundException($"{shardingEntityType}");
            return virtualTable;
        }

        public IVirtualTable GetVirtualTable(string originalTableName)
        {
            return _virtualTables.Values.FirstOrDefault(o => o.GetOriginalTableName() == originalTableName) ?? throw new CreateSqlVirtualTableNotFoundException(originalTableName);
        }

        /// <summary>
        /// 添加虚拟表
        /// </summary>
        /// <param name="virtualTable"></param>
        public void AddVirtualTable(IVirtualTable virtualTable)
        {
            _virtualTables.TryAdd(virtualTable.EntityType, virtualTable);
        }

        /// <summary>
        /// 添加物理表
        /// </summary>
        /// <param name="virtualTable"></param>
        /// <param name="physicTable"></param>
        public void AddPhysicTable(IVirtualTable virtualTable, IPhysicTable physicTable)
        {
            AddPhysicTable(virtualTable.EntityType, physicTable);
        }

        /// <summary>
        /// 添加物理表
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <param name="physicTable"></param>
        public void AddPhysicTable(Type shardingEntityType, IPhysicTable physicTable)
        {
            var virtualTable = GetVirtualTable(shardingEntityType);
            virtualTable.AddPhysicTable(physicTable);
        }
        /// <summary>
        /// 是否是分表字段
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <param name="shardingField"></param>
        /// <returns></returns>
        public bool IsShardingKey(Type shardingEntityType, string shardingField)
        {
            return _virtualTables.TryGetValue(shardingEntityType, out var virtualTable) && virtualTable.ShardingConfig.ShardingField == shardingField;
        }
    }
}