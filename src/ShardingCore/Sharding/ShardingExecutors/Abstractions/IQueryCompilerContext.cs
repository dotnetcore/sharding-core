using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.Abstractions
{
    public interface IQueryCompilerContext
    {
        ISet<Type> GetQueryEntities();
        IShardingDbContext GetShardingDbContext();
        Expression GetQueryExpression();
        IEntityMetadataManager GetEntityMetadataManager();
        Type GetShardingDbContextType();
        QueryCompilerExecutor GetQueryCompilerExecutor();
        bool IsEnumerableQuery();
        bool IsParallelQuery()；
    }
}
