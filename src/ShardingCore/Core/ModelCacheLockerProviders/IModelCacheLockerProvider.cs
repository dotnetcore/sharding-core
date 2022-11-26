namespace ShardingCore.Core.ModelCacheLockerProviders
{
    public interface IModelCacheLockerProvider
    {
        object GetCacheLockObject(object modelCacheKey);
    }
}
