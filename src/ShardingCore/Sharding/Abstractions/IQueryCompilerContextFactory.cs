using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Parsers.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.Abstractions
{
    public interface IQueryCompilerContextFactory
    {
        IQueryCompilerContext Create(IPrepareParseResult prepareParseResult);
    }
}
