using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Core.NotSupportShardingProviders
{
    public interface INotSupportShardingProvider
    {
        void CheckNotSupportSharding(IQueryCompilerContext queryCompilerContext);
        bool IsNotSupportSharding(IQueryCompilerContext queryCompilerContext);
    }
}
