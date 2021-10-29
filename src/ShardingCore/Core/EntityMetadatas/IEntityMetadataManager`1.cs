using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 分片 对象元数据信息管理
    /// </summary>
    /// <typeparam name="TShardingDbContext"></typeparam>
    public interface IEntityMetadataManager<TShardingDbContext>: IEntityMetadataManager where TShardingDbContext:DbContext,IShardingDbContext
    {
    }
}
