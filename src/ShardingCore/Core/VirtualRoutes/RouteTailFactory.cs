using System.Linq;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 22 August 2021 14:58:58
    * @Email: 326308290@qq.com
    */
    public class RouteTailFactory : IRouteTailFactory
    {
        private readonly IEntityMetadataManager _entityMetadataManager;

        public RouteTailFactory(IEntityMetadataManager entityMetadataManager)
        {
            _entityMetadataManager = entityMetadataManager;
        }

        public IRouteTail Create(string tail)
        {
            return Create(tail, true);
        }

        public IRouteTail Create(string tail, bool cache)
        {
            if (cache)
            {
                return new SingleQueryRouteTail(tail);
            }
            else
            {
                return new NoCacheSingleQueryRouteTail(tail);
            }
        }

        public IRouteTail Create(TableRouteResult tableRouteResult)
        {
            return Create(tableRouteResult, true);
        }

        public IRouteTail Create(TableRouteResult tableRouteResult, bool cache)
        {
            if (tableRouteResult == null || tableRouteResult.ReplaceTables.IsEmpty())
            {
                if (cache)
                {
                    return new SingleQueryRouteTail(string.Empty);
                }
                else
                {
                    return new NoCacheSingleQueryRouteTail(string.Empty);
                }
            }

            if (tableRouteResult.ReplaceTables.Count == 1)
            {
                if (cache)
                {
                    return new SingleQueryRouteTail(tableRouteResult);
                }
                else
                {
                    return new NoCacheSingleQueryRouteTail(tableRouteResult);
                }
            }

            var isShardingTableQuery = tableRouteResult.ReplaceTables.Select(o => o.EntityType)
                .Any(o => _entityMetadataManager.IsShardingTable(o));
            return new MultiQueryRouteTail(tableRouteResult, isShardingTableQuery);
        }
    }
}