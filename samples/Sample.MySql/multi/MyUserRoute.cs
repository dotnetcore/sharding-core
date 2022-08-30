using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.MySql.multi;

public class MyUserRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<MyUser>
{
    protected override bool EnableHintRoute => true;

    public MyUserRoute() : base(1, 3)
    {
    }

    public override void Configure(EntityMetadataTableBuilder<MyUser> builder)
    {
        builder.ShardingProperty(o => o.Id);
    }
}