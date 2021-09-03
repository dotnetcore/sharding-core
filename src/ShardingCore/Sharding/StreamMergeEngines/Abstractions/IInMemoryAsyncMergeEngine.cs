using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.StreamMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/19 8:58:47
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IInMemoryAsyncMergeEngine<TEntity>
    {
        StreamMergeContext<TEntity> GetStreamMergeContext();
        Task<List<RouteQueryResult<TResult>>> ExecuteAsync<TResult>(Func<IQueryable, Task<TResult>> efQuery,
            CancellationToken cancellationToken = new CancellationToken());
    }
}
