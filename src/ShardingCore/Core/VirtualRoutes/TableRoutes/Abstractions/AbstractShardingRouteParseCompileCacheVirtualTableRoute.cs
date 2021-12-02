using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{
    public class AbstractShardingRouteParseCompileCacheVirtualTableRoute<TEntity, TKey> : AbstractShardingFilterVirtualTableRoute<TEntity, TKey> where TEntity : class
    {
        public override string ShardingKeyToTail(object shardingKey)
        {
            throw new NotImplementedException();
        }

        public override IPhysicTable RouteWithValue(List<IPhysicTable> allPhysicTables, object shardingKey)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetAllTails()
        {
            throw new NotImplementedException();
        }

        public override void Configure(EntityMetadataTableBuilder<TEntity> builder)
        {
            throw new NotImplementedException();
        }

        protected override List<IPhysicTable> DoRouteWithPredicate(List<IPhysicTable> allPhysicTables, IQueryable queryable)
        {
            throw new NotImplementedException();
        }
    }
}
