using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeContexts
{
    internal class RewriteResult:IRewriteResult
    {
        private readonly IQueryable _combineQueryable;
        private readonly IQueryable _rewriteQueryable;

        public RewriteResult(IQueryable combineQueryable,IQueryable rewriteQueryable)
        {
            _combineQueryable = combineQueryable;
            _rewriteQueryable = rewriteQueryable;
        }
        public IQueryable GetCombineQueryable()
        {
            return _combineQueryable;
        }

        public IQueryable GetRewriteQueryable()
        {
            return _rewriteQueryable;
        }
    }
}
