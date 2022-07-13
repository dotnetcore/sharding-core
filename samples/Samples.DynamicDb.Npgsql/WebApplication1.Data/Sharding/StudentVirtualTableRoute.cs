using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;
using WebApplication1.Data.Models;

namespace WebApplication1.Data.Sharding
{

    public class StudentVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<Student>
    {
        public StudentVirtualTableRoute() : base(3, 6)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<Student> builder)
        {
            builder.ShardingProperty(o => o.Name);
        }
    }

}
