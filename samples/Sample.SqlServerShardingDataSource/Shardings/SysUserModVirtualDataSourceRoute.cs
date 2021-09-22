using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sample.SqlServerShardingDataSource.Domain.Entities;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingCore.Helpers;

namespace Sample.SqlServerShardingDataSource.Shardings
{
    public class SysUserModVirtualDataSourceRoute:AbstractShardingOperatorVirtualDataSourceRoute<SysUserMod,string>
    {
        protected readonly int Mod=3;
        protected readonly int TailLength=1;
        protected readonly char PaddingChar='0';

        protected override string ConvertToShardingKey(object shardingKey)
        {
            return shardingKey.ToString();
        }

        public override string ShardingKeyToDataSourceName(object shardingKey)
        {
            var shardingKeyStr = ConvertToShardingKey(shardingKey);
            return "ds"+Math.Abs(ShardingCoreHelper.GetStringHashCode(shardingKeyStr) % Mod).ToString().PadLeft(TailLength, PaddingChar); ;
        }

        public override List<string> GetAllDataSourceNames()
        {
            return new List<string>()
            {
                "ds0",
                "ds1",
                "ds2"
            };
        }

        public override bool AddDataSourceName(string dataSourceName)
        {
            throw new NotImplementedException();
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
    }
}
