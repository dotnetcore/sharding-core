using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

namespace Sample.MySql.Shardings;

public class DynamicTableRoute:AbstractShardingOperatorVirtualTableRoute<DynamicTable,string>
{
    public override string ShardingKeyToTail(object shardingKey)
    {
        return $"{shardingKey}";
    }

    public override List<string> GetTails()
    {
        return new List<string>();
    }

    public override void Configure(EntityMetadataTableBuilder<DynamicTable> builder)
    {
        builder.ShardingProperty(o => o.Id);
    }

    public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
    {
        var shardingKeyToTail = ShardingKeyToTail(shardingKey);
        switch (shardingOperator)
        {
            case ShardingOperatorEnum.Equal: return t => t == shardingKeyToTail;
            default: return t => true;
        }
    }
}