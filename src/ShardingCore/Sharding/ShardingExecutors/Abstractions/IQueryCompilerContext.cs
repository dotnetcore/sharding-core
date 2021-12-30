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

        /// <summary>
        /// 当前是否读写分离走读库(包括是否启用读写分离和是否当前的dbcontext启用了读库查询)
        /// </summary>
        /// <returns></returns>
        bool CurrentQueryReadConnection();
        /// <summary>
        /// 是否是未追踪查询
        /// </summary>
        /// <returns></returns>
        bool IsQueryTrack();
    }
}
