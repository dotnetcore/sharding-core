using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    public sealed class PhysicDataSourcePool:IPhysicDataSourcePool
    {
        private readonly ConcurrentDictionary<string, IPhysicDataSource> _physicDataSources =
            new ConcurrentDictionary<string, IPhysicDataSource>();
        public bool TryAdd(IPhysicDataSource physicDataSource)
        {
            return _physicDataSources.TryAdd(physicDataSource.DataSourceName, physicDataSource);
        }

        public IPhysicDataSource TryGet(string dataSourceName)
        {
            if (dataSourceName == null) return null;
            if (_physicDataSources.TryGetValue(dataSourceName, out var physicDataSource))
                return physicDataSource;
            return null;
        }

        public List<string> GetAllDataSourceNames()
        {
            return _physicDataSources.Keys.ToList();
        }

        public IDictionary<string, string> GetDataSources()
        {
            return _physicDataSources.ToDictionary(k => k.Key, k => k.Value.ConnectionString);
        }
    }
}
