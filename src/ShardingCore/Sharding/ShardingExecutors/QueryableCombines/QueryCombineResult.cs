using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors.QueryableCombines
{
    public class QueryCombineResult
    {
        private readonly IQueryable _queryable;
        private readonly IQueryCompilerContext _queryCompilerContext;

        public QueryCombineResult(IQueryable queryable,IQueryCompilerContext queryCompilerContext)
        {
            _queryable = queryable;
            _queryCompilerContext = queryCompilerContext;
        }
        public IQueryable GetCombineQueryable()
        {
            return _queryable;
        }

        public IQueryCompilerContext GetQueryCompilerContext()
        {
            return _queryCompilerContext;
        }
    }
}
