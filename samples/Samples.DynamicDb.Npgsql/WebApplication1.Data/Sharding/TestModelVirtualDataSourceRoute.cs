using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Data.Helpers;
using WebApplication1.Data.Models;

namespace WebApplication1.Data.Sharding
{

    public class TestModelVirtualDataSourceRoute : AbstractShardingOperatorVirtualDataSourceRoute<TestModel, string>
    {

        /// <summary>
        /// 配置文件名称
        /// </summary>
        public const string ConfigFileName = "muitDbConfig.json";
        private List<string> _dataSources = new() { };

        protected string ConvertToShardingKey(object shardingKey)
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
            _dataSources = JsonFileHelper.Read<List<string>>(AppContext.BaseDirectory, ConfigFileName);
            return _dataSources;
        }

        public override bool AddDataSourceName(string dataSourceName)
        {
            if (_dataSources.Any(o => o == dataSourceName)) return false;
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

        public override void Configure(EntityMetadataDataSourceBuilder<TestModel> builder)
        {
            builder.ShardingProperty(o => o.TestNewField);
        }

    }
}
