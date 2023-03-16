using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;

namespace Sample.MySql.Shardings
{
    public class SysUserLogByMonthDSRoute:AbstractShardingOperatorVirtualDataSourceRoute<SysUserLogByMonth,DateTime>
    {
        public override string ShardingKeyToDataSourceName(object shardingKey)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetAllDataSourceNames()
        {
            throw new NotImplementedException();
        }

        public override bool AddDataSourceName(string dataSourceName)
        {
            throw new NotImplementedException();
        }

        public override void Configure(EntityMetadataDataSourceBuilder<SysUserLogByMonth> builder)
        {
            throw new NotImplementedException();
        }

        public override Func<string, bool> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            throw new NotImplementedException();
        }
    }
}
