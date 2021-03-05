using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;

namespace ShardingCore.Core.ShardingDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 19 February 2021 08:07:27
* @Email: 326308290@qq.com
*/
    public class ShardingDataSourceManager : IShardingDataSourceManager
    {
        private readonly Dictionary<Type, List<ShardingDataSourceEntry>> _entityMaps = new Dictionary<Type, List<ShardingDataSourceEntry>>();
        private readonly Dictionary<ShardingDataSourceDbEntry, List<ShardingDataSourceEntry>> _dbContextMaps = new Dictionary<ShardingDataSourceDbEntry, List<ShardingDataSourceEntry>>();

        /// <summary>
        /// 添加数据源
        /// </summary>
        /// <param name="connectKey"></param>
        /// <param name="connectionString"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDbContext"></typeparam>
        public void AddDataSource<T, TDbContext>(string connectKey, string connectionString) where T : IShardingDataSource where TDbContext : DbContext
        {
            var entityType = typeof(T);
            var dbContextType = typeof(TDbContext);
            var dataSourceDbEntry = new ShardingDataSourceDbEntry(connectKey, dbContextType, connectionString);
            if (!_entityMaps.ContainsKey(entityType))
            {
                _entityMaps.Add(dbContextType, new List<ShardingDataSourceEntry>());
            }

            if (!_dbContextMaps.ContainsKey(dataSourceDbEntry))
            {
                _dbContextMaps.Add(dataSourceDbEntry, new List<ShardingDataSourceEntry>());
            }

            _entityMaps[entityType].Add(new ShardingDataSourceEntry(entityType, dataSourceDbEntry));
            _dbContextMaps[dataSourceDbEntry].Add(new ShardingDataSourceEntry(entityType, dataSourceDbEntry));
        }

        public bool IsShardingDataSource()
        {
            return _dbContextMaps.Count > 1;
        }

        public IEnumerable<ShardingDataSourceDbEntry> FilterDataSources(ISet<Type> queryEntities)
        {
            var list = _entityMaps.Where(o => queryEntities.Contains(o.Key)).Select(o => o.Value.Select(o => o.DataSourceDbEntry).ToList()).ToList();
            if (list.Count <= 0)
                return new List<ShardingDataSourceDbEntry>(0);
            if (list.Count == 1)
                return list[0];
            return list.Aggregate((pre, next) => pre.Except(next).ToList());
        }
    }
}