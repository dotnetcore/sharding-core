using System;
using ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions;

namespace ShardingCore.Core.UnionAllMergeShardingProviders
{
    public class UnionAllMergeScope:IDisposable
    {
        public IUnionAllMergeAccessor UnionAllMergeAccessor { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unionAllMergeAccessor"></param>
        public UnionAllMergeScope(IUnionAllMergeAccessor unionAllMergeAccessor)
        {
            UnionAllMergeAccessor = unionAllMergeAccessor;
        }
        public void Dispose()
        {
            UnionAllMergeAccessor.SqlSupportContext = null;
        }
    }
}
