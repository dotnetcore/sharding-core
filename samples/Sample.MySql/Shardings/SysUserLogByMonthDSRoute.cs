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
            //只返回当前和历史
        private  static readonly List<string> tails = new List<string>()
        {
            "current",
            "history"
        };
        public override List<string> GetAllDataSourceNames()
        { 
            return tails;
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
            //判断过滤查询历史还是现在
            throw new NotImplementedException();
        }
    }
}
