namespace ShardingCore.Core.UnionAllMergeShardingProviders.Abstractions
{
    public interface IUnionAllMergeAccessor
    {
        UnionAllMergeContext SqlSupportContext { get; set; }
    }
}
