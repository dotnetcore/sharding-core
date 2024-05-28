using Sample.MySql.DbContexts;
using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingCore.Helpers;

namespace Sample.MySql.Shardings
{
    public class SysUserLogByMonthDSRoute:AbstractShardingOperatorVirtualDataSourceRoute<SysUserLogByMonth,DateTime>
    {
        private readonly IShardingProvider _shardingProvider;

        public SysUserLogByMonthDSRoute(IShardingProvider shardingProvider)
        {
            _shardingProvider = shardingProvider;
        }
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

        // public override string RouteWithValue(object shardingKey)
        // {
        //     //计算出数据源名
        //     var dataSourceName = ShardingKeyToDataSourceName(shardingKey);
        //     var shardingRuntimeContext = _shardingProvider.ApplicationServiceProvider.GetRequiredService<IShardingRuntimeContext<DefaultShardingDbContext>>();
        //     DynamicShardingHelper.DynamicAppendDataSource(shardingRuntimeContext,dataSourceName,$"server=127.0.0.1;port=3306;database=db_{dataSourceName};userid=root;password=root;",true,true);
        // }
    }
}
