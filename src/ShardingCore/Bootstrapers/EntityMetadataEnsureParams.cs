using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ShardingCore.Bootstrapers
{
    public class EntityMetadataEnsureParams
    {
        public EntityMetadataEnsureParams(IEntityType entityType)
        {
            EntityType = entityType;

#if !EFCORE2
            var virtualTableName = entityType.GetTableName();
#endif
#if EFCORE2
            var virtualTableName = entityType.Relational().TableName;
#endif
            VirtualTableName = virtualTableName;
        }

        public IEntityType EntityType { get; }
        public string VirtualTableName { get; }
    }
}
