using Microsoft.EntityFrameworkCore.Metadata;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Extensions.InternalExtensions;

/*
* @Author: xjm
* @Description:
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrappers
{
    /// <summary>
    /// 分表、分库对象确定参数用来初始化<see cref="EntityMetadata"/>时所需的信息
    /// </summary>
    public class EntityMetadataEnsureParams
    {
        public EntityMetadataEnsureParams(IEntityType entityType)
        {
            EntityType = entityType;

            VirtualTableName = entityType.GetEntityTypeTableName();
        }

        public IEntityType EntityType { get; }
        public string VirtualTableName { get; }
    }
}
