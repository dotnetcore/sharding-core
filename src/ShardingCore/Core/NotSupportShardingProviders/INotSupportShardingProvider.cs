using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Core.NotSupportShardingProviders
{
    [Obsolete("plz use NotSupport() eg. dbcontext.Set<User>().NotSupport().Where(...).ToList()")]
    public interface INotSupportShardingProvider
    {
        [Obsolete("plz use NotSupport() eg. dbcontext.Set<User>().NotSupport().Where(...).ToList()")]
        void CheckNotSupportSharding(IQueryCompilerContext queryCompilerContext);
        [Obsolete("plz use NotSupport() eg. dbcontext.Set<User>().NotSupport().Where(...).ToList()")]
        bool IsNotSupportSharding(IQueryCompilerContext queryCompilerContext);
    }
}
