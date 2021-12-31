using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.QueryTrackers
{
    public class QueryTracker : IQueryTracker
    {
        public object Track(object entity, IShardingDbContext shardingDbContext)
        {
            var genericDbContext = shardingDbContext.CreateGenericDbContext(entity);
            var attachedEntity = genericDbContext.GetAttachedEntity(entity);
            if (attachedEntity == null)
                genericDbContext.Attach(entity);
            else
            {
                return attachedEntity;
            }

            return null;
        }
    }
}
