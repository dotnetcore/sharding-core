using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.MergeEngines.Common;

namespace ShardingCore.Sharding.MergeEngines.Executors.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 11:45:21
    /// Email: 326308290@qq.com
    internal interface IExecutor<TResult>
    {
        Task<LinkedList<TResult>> ExecuteAsync(bool async, DataSourceSqlExecutorUnit dataSourceSqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken());
    }
}
