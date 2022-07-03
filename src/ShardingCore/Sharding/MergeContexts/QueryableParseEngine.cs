using System;
using System.Linq;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

/*
* @Author: xjm
* @Description:
* @Date: DATE TIME
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Sharding.MergeContexts
{
    public class QueryableParseEngine:IQueryableParseEngine
    {
        public IParseResult Parse(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            var isEnumerableQuery = mergeQueryCompilerContext.IsEnumerableQuery();
            string queryMethodName = mergeQueryCompilerContext.GetQueryMethodName();
            var combineQueryable = mergeQueryCompilerContext.GetQueryCombineResult().GetCombineQueryable();
            var queryableExtraDiscoverVisitor = new QueryableExtraDiscoverVisitor(mergeQueryCompilerContext);
            queryableExtraDiscoverVisitor.Visit(combineQueryable.Expression);
            return new ParseResult(queryableExtraDiscoverVisitor.GetPaginationContext(),
                queryableExtraDiscoverVisitor.GetOrderByContext(), queryableExtraDiscoverVisitor.GetSelectContext(),
                queryableExtraDiscoverVisitor.GetGroupByContext(),isEnumerableQuery,queryMethodName);
        }
    }
}