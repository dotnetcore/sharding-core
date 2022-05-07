using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerators
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 13:10:34
    /// Email: 326308290@qq.com
    internal class EmptyQueryEnumeratorExecutor<TResult> : AbstractEnumeratorExecutor<TResult>
    {
        private readonly IStreamMergeCombine _streamMergeCombine;
        private readonly bool _async;

        public EmptyQueryEnumeratorExecutor(StreamMergeContext streamMergeContext, IStreamMergeCombine streamMergeCombine, bool async) : base(streamMergeContext)
        {
            _streamMergeCombine = streamMergeCombine;
            _async = async;
        }

        protected override Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return _streamMergeCombine;
        }
    }
}
