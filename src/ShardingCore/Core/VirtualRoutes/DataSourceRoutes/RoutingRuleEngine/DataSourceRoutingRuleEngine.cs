using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RoutingRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/2 16:20:37
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DataSourceRoutingRuleEngine: IDataSourceRoutingRuleEngine
    {
        private readonly IVirtualDataSourceManager _virtualDataSourceManager;

        public DataSourceRoutingRuleEngine(IVirtualDataSourceManager virtualDataSourceManager)
        {
            _virtualDataSourceManager = virtualDataSourceManager;
        }

        public DataSourceRoutingResult Route<T>(DataSourceRoutingRuleContext<T> routingRuleContext)
        {
            var queryEntities = routingRuleContext.Queryable.ParseQueryableRoute().Where(o=>o.IsShardingDataSource()).ToList();
            if (queryEntities.IsEmpty())
                return new DataSourceRoutingResult(_virtualDataSourceManager.GetAllShardingConnectKeys());

            var dataSourceMaps = new Dictionary<Type, ISet<string>>();
            foreach (var queryEntity in queryEntities)
            {
                if(!_virtualDataSourceManager.HasVirtualShardingDataSourceRoute(queryEntity))
                    continue;
                var virtualDataSource = _virtualDataSourceManager.GetVirtualDataSource(queryEntity);
                var dataSourceConfigs = virtualDataSource.RouteTo(new VirutalDataSourceRouteConfig(routingRuleContext.Queryable));
                if (!dataSourceMaps.ContainsKey(queryEntity))
                {
                    dataSourceMaps.Add(queryEntity, dataSourceConfigs.ToHashSet());
                }
                else
                {
                    foreach (var shardingDataSource in dataSourceConfigs)
                    {
                        dataSourceMaps[queryEntity].Add(shardingDataSource);
                    }
                }
            }

            if (dataSourceMaps.IsEmpty())
                return new DataSourceRoutingResult(new HashSet<string>());
            if(dataSourceMaps.Count==1)
                return new DataSourceRoutingResult(dataSourceMaps.FirstOrDefault().Value); 
            var intersect = dataSourceMaps.Select(o => o.Value).Aggregate((p, n) => p.Intersect(n).ToHashSet());
            return new DataSourceRoutingResult(intersect);
        }
    }
}
