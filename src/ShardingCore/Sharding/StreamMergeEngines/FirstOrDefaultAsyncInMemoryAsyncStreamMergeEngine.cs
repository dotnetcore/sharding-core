using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 15:16:36
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class FirstOrDefaultAsyncInMemoryAsyncStreamMergeEngine<TResult>:AbstractInMemoryAsyncStreamMergeEngine<TResult>
    {
        private readonly StreamMergeContext<TResult> _mergeContext;

        public FirstOrDefaultAsyncInMemoryAsyncStreamMergeEngine(StreamMergeContext<TResult> mergeContext) : base(mergeContext)
        {
            _mergeContext = mergeContext;
        }
        public async Task<TResult> ExecuteAsync(Func<IQueryable, Task<TResult>> efQuery, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync(efQuery, cancellationToken);
            var q = result.Where(o => o != null).AsQueryable();
            if (_mergeContext.Orders.Any())
                return q.OrderWithExpression(_mergeContext.Orders).FirstOrDefault();

            return q.FirstOrDefault();
        }
    }
}
