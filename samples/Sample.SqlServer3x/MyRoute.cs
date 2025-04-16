using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

namespace Sample.SqlServer3x;

public class MyRoute<T,TKey> : AbstractShardingFilterVirtualTableRoute<T,TKey> where T : class
{
    public override string ShardingKeyToTail(object shardingKey)
    {
        var type = typeof(T);
        throw new System.NotImplementedException();
    }

    public override TableRouteUnit RouteWithValue(DataSourceRouteResult dataSourceRouteResult, object shardingKey)
    {
        var type = typeof(T);
        throw new System.NotImplementedException();
    }

    public override List<string> GetTails()
    {
        var type = typeof(T);
        throw new System.NotImplementedException();
    }

    public override void Configure(EntityMetadataTableBuilder<T> builder)
    {
        var type = typeof(T);
        throw new System.NotImplementedException();
    }

    protected override List<TableRouteUnit> DoRouteWithPredicate(DataSourceRouteResult dataSourceRouteResult, IQueryable queryable)
    {
        var type = typeof(T);
        throw new System.NotImplementedException();
    }
}