using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using ShardingCore.Extensions;

namespace ShardingCore.EFCores
{
    public class ShardingDbSetInitializer:DbSetInitializer
    {
#if !EFCORE2
        public ShardingDbSetInitializer(IDbSetFinder setFinder, IDbSetSource setSource) : base(setFinder, setSource)
        {
        }
#endif
#if EFCORE2
        public ShardingDbSetInitializer(IDbSetFinder setFinder, IDbSetSource setSource, IDbQuerySource querySource) : base(setFinder, setSource, querySource)
        {
        }
#endif
        public override void InitializeSets(DbContext context)
        {
            base.InitializeSets(context);
            if (context.IsShellDbContext())
            {
                context.GetShardingRuntimeContext().GetOrCreateShardingRuntimeModel(context);
            }
        }
    }
}
