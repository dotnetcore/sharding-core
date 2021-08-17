using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 22:36:14
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class CountAsyncInMemoryAsyncStreamMergeEngine<TResult> : AbstractInMemoryAsyncStreamMergeEngine<TResult>
    {
        private readonly StreamMergeContext<TResult> _mergeContext;

        public CountAsyncInMemoryAsyncStreamMergeEngine(StreamMergeContext<TResult> mergeContext) : base(mergeContext)
        {
            _mergeContext = mergeContext;
        }
        public async Task<int> DoExecuteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.ExecuteAsync(async iqueryable => await EntityFrameworkQueryableExtensions.CountAsync((IQueryable<TResult>)iqueryable, cancellationToken), cancellationToken);

            return result.Sum();
        }

    }
}