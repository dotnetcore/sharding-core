using System;
using System.Collections.Concurrent;
using MySqlConnector;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.TableCreator;

/*
* @Author: xjm
* @Description:
* @Date: DATE
* @Email: 326308290@qq.com
*/
namespace Sample.AutoCreateIfPresent
{
    public class OrderByHourRoute : AbstractShardingOperatorVirtualTableRoute<OrderByHour, DateTime>
    {
        private const string Tables = "Tables";
        private const string TABLE_SCHEMA = "TABLE_SCHEMA";
        private const string TABLE_NAME = "TABLE_NAME";

        private const string CurrentTableName = nameof(OrderByHour);
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly IShardingTableCreator _shardingTableCreator;
        private readonly ConcurrentDictionary<string, object?> _tails = new ConcurrentDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new object();

        public OrderByHourRoute(IVirtualDataSource virtualDataSource, IShardingTableCreator shardingTableCreator)
        {
            _virtualDataSource = virtualDataSource;
            _shardingTableCreator = shardingTableCreator;
            InitTails();
        }

        private void InitTails()
        {
            
            //启动寻找有哪些表后缀
            using (var connection = new MySqlConnection(_virtualDataSource.DefaultConnectionString))
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
                            var tableName = dataTable.Rows[i][TABLE_NAME]?.ToString()??string.Empty;
                            if (tableName.StartsWith(CurrentTableName, StringComparison.OrdinalIgnoreCase))
                            {
                                //如果没有下划线那么需要CurrentTableName.Length有下划线就要CurrentTableName.Length+1
                                _tails.TryAdd(tableName.Substring(CurrentTableName.Length+1),null);
                            }
                        }
                    }
                }
            }
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            var dateTime = (DateTime)shardingKey;
            return ShardingKeyFormat(dateTime);
        }
        private string ShardingKeyFormat(DateTime dateTime)
        {
            var tail = $"{dateTime:yyyyMMddHH}";

            return tail;
        }

        /// <summary>
        /// 如果你是非mysql数据库请自行实现这个方法返回当前类在数据库已经存在的后缀
        /// 仅启动时调用
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTails()
        {
            return _tails.Keys.ToList();
        }

        public override void Configure(EntityMetadataTableBuilder<OrderByHour> builder)
        {
            builder.ShardingProperty(o => o.CreateTime);
        }

        public override Func<string, bool> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyFormat(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var currentHourBeginTime = new DateTime(shardingKey.Year,shardingKey.Month,shardingKey.Day,shardingKey.Hour,0,0);
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (currentHourBeginTime == shardingKey)
                        return tail => String.Compare(tail, t, StringComparison.Ordinal) < 0;
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
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

        public override TableRouteUnit RouteWithValue(DataSourceRouteResult dataSourceRouteResult, object shardingKey)
        {
            var shardingKeyToTail = ShardingKeyToTail(shardingKey);

            if (!_tails.TryGetValue(shardingKeyToTail, out var _))
            {
                lock (_lock)
                {
                    if (!_tails.TryGetValue(shardingKeyToTail, out var _))
                    {
                        try
                        {
                            _shardingTableCreator.CreateTable<OrderByHour>(_virtualDataSource.DefaultDataSourceName,
                                shardingKeyToTail);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("尝试添加表失败" + ex);
                        }

                        _tails.TryAdd(shardingKeyToTail, null);
                    }
                }
            }

            return base.RouteWithValue(dataSourceRouteResult, shardingKey);
        }
    }
}