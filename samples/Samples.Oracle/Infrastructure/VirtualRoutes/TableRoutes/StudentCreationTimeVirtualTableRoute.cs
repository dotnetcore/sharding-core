using Samples.Oracle.Domain;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Days;

namespace Samples.Oracle.Infrastructure.VirtualRoutes.TableRoutes;

public class StudentCreationTimeVirtualTableRoute : AbstractSimpleShardingDayKeyDateTimeVirtualTableRoute<Student>
{
    public override void Configure(EntityMetadataTableBuilder<Student> builder)
    {
        builder.ShardingProperty(p => p.CreationTime);
    }

    public override DateTime GetBeginTime() => new(2023, 4, 20);

    public override bool AutoCreateTableByTime() => true;
}
