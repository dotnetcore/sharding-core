using System.Threading;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;

namespace ShardingCore.Core.UnionAllMergeShardingProviders
{
    public class UnionAllMergeAccessor: IUnionAllMergeAccessor
    {
        private static AsyncLocal<UnionAllMergeContext> _sqlSupportContext = new AsyncLocal<UnionAllMergeContext>();


        public UnionAllMergeContext SqlSupportContext
        {
            get => _sqlSupportContext.Value;
            set => _sqlSupportContext.Value = value;
        }
    }
}
