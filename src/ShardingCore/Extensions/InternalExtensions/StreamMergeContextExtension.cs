using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding;

namespace ShardingCore.Extensions.InternalExtensions
{
    internal  static class StreamMergeContextExtension
    {
        public static bool IsSeqQuery<TEntity>( this StreamMergeContext<TEntity> streamMergeContext)
        {
            return streamMergeContext.EntitySeqQueryConfig != null;
        }
        public static bool IsParallelExecute<TEntity>( this StreamMergeContext<TEntity> streamMergeContext)
        {
            return streamMergeContext.TableRouteResults.Length <= streamMergeContext.GetMaxQueryConnectionsLimit();
        }
    }
}
