using ShardingCore.Core.CustomerDatabaseProcessers;

namespace ShardingCore.Core.NotSupportShardingProviders.Abstractions
{
    public interface INotSupportAccessor
    {
        NotSupportContext SqlSupportContext { get; set; }
    }
}
