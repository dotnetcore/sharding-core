using System.Collections.Concurrent;
using System.Linq.Expressions;
using MySqlConnector;
using Newtonsoft.Json;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.TableCreator;

namespace Sample.AutoCreateIfPresent
{
    public class AreaDeviceRoute : AbstractShardingOperatorVirtualTableRoute<AreaDevice, string>
    {
        private const string Tables = "Tables";
        private const string TABLE_SCHEMA = "TABLE_SCHEMA";
        private const string TABLE_NAME = "TABLE_NAME";

        private const string CurrentTableName = nameof(AreaDevice);
        private readonly IVirtualDataSourceManager<DefaultDbContext> _virtualDataSourceManager;
        private readonly IVirtualTableManager<DefaultDbContext> _virtualTableManager;
        private readonly IShardingTableCreator<DefaultDbContext> _shardingTableCreator;
        private readonly ConcurrentDictionary<string, object?> _tails = new ConcurrentDictionary<string, object?>();
        private readonly object _lock = new object();

        public AreaDeviceRoute(IVirtualDataSourceManager<DefaultDbContext> virtualDataSourceManager, IVirtualTableManager<DefaultDbContext> virtualTableManager, IShardingTableCreator<DefaultDbContext> shardingTableCreator)
        {
            _virtualDataSourceManager = virtualDataSourceManager;
            _virtualTableManager = virtualTableManager;
            _shardingTableCreator = shardingTableCreator;
        }
        

        public override string ShardingKeyToTail(object shardingKey)
        {
            return $"{shardingKey}";
        }
        /// <summary>
        /// 如果你是非mysql数据库请自行实现这个方法返回当前类在数据库已经存在的后缀
        /// 仅启动时调用
        /// </summary>
        /// <returns></returns>
        public override List<string> GetAllTails()
        {
            //启动寻找有哪些表后缀
            using (var connection = new MySqlConnection(_virtualDataSourceManager.GetCurrentVirtualDataSource().DefaultConnectionString))
            {
                connection.Open();
                var database = connection.Database;

                using (var dataTable = connection.GetSchema(Tables))
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        var schema = dataTable.Rows[i][TABLE_SCHEMA];
                        if (database.Equals($"{schema}", StringComparison.OrdinalIgnoreCase))
                        {
                            var tableName = dataTable.Rows[i][TABLE_NAME]?.ToString() ?? string.Empty;
                            if (tableName.StartsWith(CurrentTableName,StringComparison.OrdinalIgnoreCase))
                            {
                                //如果没有下划线那么需要CurrentTableName.Length有下划线就要CurrentTableName.Length+1
                                _tails.TryAdd(tableName.Substring(CurrentTableName.Length+1), null);
                            }
                        }
                    }
                }
            }
            return _tails.Keys.ToList();
        }

        public override void Configure(EntityMetadataTableBuilder<AreaDevice> builder)
        {
            builder.ShardingProperty(o => o.Area);
        }

        public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                    {
#if DEBUG
                        Console.WriteLine($"shardingOperator is not equal scan all table tail");
#endif
                        return tail => true;
                    }
            }
        }

        public override IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, object shardingKey)
        {
            var shardingKeyToTail = ShardingKeyToTail(shardingKey);

            if (!_tails.TryGetValue(shardingKeyToTail, out var _))
            {
                lock (_lock)
                {
                    if (!_tails.TryGetValue(shardingKeyToTail, out var _))
                    {
                        var virtualTable = _virtualTableManager.GetVirtualTable(typeof(AreaDevice));
                        _virtualTableManager.AddPhysicTable(virtualTable, new DefaultPhysicTable(virtualTable, shardingKeyToTail));
                        try
                        {
                            _shardingTableCreator.CreateTable<AreaDevice>(_virtualDataSourceManager.GetCurrentVirtualDataSource().DefaultDataSourceName, shardingKeyToTail);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("尝试添加表失败" + ex);
                        }

                        _tails.TryAdd(shardingKeyToTail, null);
                    }
                }
            }

            var needRefresh = allPhysicTables.Count != _tails.Count;
            if (needRefresh)
            {
                var virtualTable = _virtualTableManager.GetVirtualTable(typeof(AreaDevice));
                //修复可能导致迭代器遍历时添加的bug
                var keys = _tails.Keys.ToList();
                foreach (var tail in keys)
                {
                    var hashSet = allPhysicTables.Select(o => o.Tail).ToHashSet();
                    if (!hashSet.Contains(tail))
                    {
                        var tables = virtualTable.GetAllPhysicTables();
                        var physicTable = tables.FirstOrDefault(o => o.Tail == tail);
                        if (physicTable != null)
                        {
                            allPhysicTables.Add(physicTable);
                        }
                    }
                }
            }
            var physicTables = allPhysicTables.Where(o => o.Tail == shardingKeyToTail).ToList();
            if (physicTables.IsEmpty())
            {
                throw new ShardingCoreException($"sharding key route not match {EntityMetadata.EntityType} -> [{EntityMetadata.ShardingTableProperty.Name}] ->【{shardingKey}】 all tails ->[{string.Join(",", allPhysicTables.Select(o => o.FullName))}]");
            }

            if (physicTables.Count > 1)
                throw new ShardingCoreException($"more than one route match table:{string.Join(",", physicTables.Select(o => $"[{o.FullName}]"))}");
            return physicTables[0];
        }
    }
}