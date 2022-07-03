using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.Abstractions
{
    public interface IQueryCompilerContext
    {
        Dictionary<Type, IQueryable> GetQueryEntities();
        IShardingDbContext GetShardingDbContext();
        Expression GetQueryExpression();
        IEntityMetadataManager GetEntityMetadataManager();
        Type GetShardingDbContextType();
        /// <summary>
        /// 如果当前是QueryCompilerContext
        /// 获取编译后的执行结果,如果当前是没有分表分库
        /// 直接返回默认的dbcontext并且替换内部所有的dbcontext为执行dbcontext，简单理解为原生查询
        /// 如果当前是MergeQueryCompilerContext
        /// 获取编译的执行结果,如果当前是有分表分库
        /// 但是如果是只涉及到一张表那么也可以简单理解为原生查询
        /// </summary>
        /// <returns></returns>
        QueryCompilerExecutor GetQueryCompilerExecutor();
        bool IsEnumerableQuery();
        string GetQueryMethodName();

        /// <summary>
        /// 当前是否读写分离走读库(包括是否启用读写分离和是否当前的dbcontext启用了读库查询)
        /// </summary>
        /// <returns></returns>
        bool IsParallelQuery();
        /// <summary>
        /// 是否是未追踪查询
        /// </summary>
        /// <returns></returns>
        bool IsQueryTrack();
        bool UseUnionAllMerge();

        int? GetMaxQueryConnectionsLimit();
        ConnectionModeEnum? GetConnectionMode();
        bool? IsSequence();
        bool? SameWithShardingComparer();
        bool IsSingleShardingEntityQuery();
        Type GetSingleShardingEntityType();
    }
}
