using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.MySql.Shardings;

public class TestModRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<TestMod>
{
    public TestModRoute() : base(2, 3)
    {
    }

    public override void Configure(EntityMetadataTableBuilder<TestMod> builder)
    {
        builder.ShardingProperty(o => o.Id);
    }
}

public class TestModItemRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<TestModItem>
{
    public TestModItemRoute()  : base(2, 3)
    {
    }

    public override void Configure(EntityMetadataTableBuilder<TestModItem> builder)
    {
        builder.ShardingProperty(o => o.MainId);
    }
}