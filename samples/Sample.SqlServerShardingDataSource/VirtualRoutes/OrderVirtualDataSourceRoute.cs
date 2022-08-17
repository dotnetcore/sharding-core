using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sample.SqlServerShardingDataSource.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingCore.Helpers;

namespace Sample.SqlServerShardingDataSource.VirtualRoutes
{
    public class OrderVirtualDataSourceRoute : AbstractShardingOperatorVirtualDataSourceRoute<Order, string>
    {
        private readonly List<string> _dataSources = Enumerable.Range(0,4).Select(o=>(o % 100).ToString().PadLeft(2,'0')).ToList();
        //我们设置区域就是数据库
        public override string ShardingKeyToDataSourceName(object shardingKey)
        {
            var shardingKeyToDataSourceName = Math.Abs(ShardingCoreHelper.GetStringHashCode(shardingKey?.ToString() ?? string.Empty) % 4).ToString().PadLeft(2, '0');
            return shardingKeyToDataSourceName;
        }

        public override List<string> GetAllDataSourceNames()
        {
            return _dataSources;
        }

        public override bool AddDataSourceName(string dataSourceName)
        {
            if (_dataSources.Any(o => o == dataSourceName))
                return false;
            _dataSources.Add(dataSourceName);
            return true;
        }

        public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {

            var t = ShardingKeyToDataSourceName(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                    {
                        return tail => true;
                    }
            }
        }

        public override void Configure(EntityMetadataDataSourceBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}
