using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class QueryCompilerExecutor
    {
        private readonly IQueryCompiler _queryCompiler;
        private readonly Expression _queryExpression;

        public QueryCompilerExecutor(DbContext dbContext,Expression queryExpression)
        {
            _queryCompiler = dbContext.GetService<IQueryCompiler>();
            _queryExpression = queryExpression.ReplaceDbContextExpression(dbContext);
        }

        public IQueryCompiler GetQueryCompiler()
        {
            return _queryCompiler;
        }

        public Expression GetReplaceQueryExpression()
        {
            return _queryExpression;
        }
    }
}
