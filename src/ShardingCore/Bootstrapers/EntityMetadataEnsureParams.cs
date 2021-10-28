using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ShardingCore.Bootstrapers
{
    public class EntityMetadataEnsureParams
    {
        public EntityMetadataEnsureParams(string dataSourceName, IEntityType entityType, string virtualTableName)
        {
            DataSourceName = dataSourceName;
            EntityType = entityType;
            VirtualTableName = virtualTableName;
        }

        public string DataSourceName { get; }
        public IEntityType EntityType { get; }
        public string VirtualTableName { get; }
    }
}
