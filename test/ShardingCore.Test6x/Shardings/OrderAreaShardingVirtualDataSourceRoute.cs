using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingCore.Test6x.Domain.Entities;

namespace ShardingCore.Test6x.Shardings
{
    public class OrderAreaShardingVirtualDataSourceRoute:AbstractShardingOperatorVirtualDataSourceRoute<Order,string>
    {
        private readonly List<string> _dataSources = new List<string>()
        {
            "A", "B", "C"
        };
        protected override string ConvertToShardingKey(object shardingKey)
        {
            return shardingKey?.ToString() ?? string.Empty;
        }
        //我们设置区域就是数据库
        public override string ShardingKeyToDataSourceName(object shardingKey)
        {
            return ConvertToShardingKey(shardingKey);
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

        protected override Expression<Func<string, bool>> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
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
            builder.ShardingProperty(o => o.Area);
        }
    }
}
