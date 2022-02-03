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
        [Obsolete("not implement this method return any value,plz use NotSupport() eg. dbcontext.Set<User>().NotSupport().Where(...).ToList()")]
        bool IsNotSupportSharding(IQueryCompilerContext queryCompilerContext);
    }
}
