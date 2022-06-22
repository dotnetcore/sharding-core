using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;

namespace Sample.MySql.Shardings;

public class SysUserModVirtualDataSourceRoute:AbstractShardingOperatorVirtualDataSourceRoute<SysUserMod,string>
{
    public override string ShardingKeyToDataSourceName(object shardingKey)
    {
        return $"{shardingKey}";
    }

    public override List<string> GetAllDataSourceNames()
    {
        return Enumerable.Range(0, 500).Select(o => $"ds{o}").ToList();
    }

    public override bool AddDataSourceName(string dataSourceName)
    {
        throw new NotImplementedException();
    }

    public override void Configure(EntityMetadataDataSourceBuilder<SysUserMod> builder)
    {
        builder.ShardingProperty(o => o.Name);
    }

    public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
    {
        var shardingKeyToDataSourceName = ShardingKeyToDataSourceName(shardingKey);
        switch (shardingOperator)
        {
            case ShardingOperatorEnum.Equal: return t => t.Equals(shardingKeyToDataSourceName);
            default: return t => true;
        }
    }
}