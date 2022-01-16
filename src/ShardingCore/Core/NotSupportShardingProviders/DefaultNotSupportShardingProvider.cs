using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Core.NotSupportShardingProviders
{
    public class DefaultNotSupportShardingProvider: INotSupportShardingProvider
    {
        public void CheckNotSupportSharding(IQueryCompilerContext queryCompilerContext)
        {
            if (IsNotSupportSharding(queryCompilerContext))
                throw new ShardingCoreInvalidOperationException(
                    $"not support sharding query :[{queryCompilerContext.GetQueryExpression().ShardingPrint()}]");
        }

        public bool IsNotSupportSharding(IQueryCompilerContext queryCompilerContext)
        {
            return queryCompilerContext.isUnion();
        }
    }
}
