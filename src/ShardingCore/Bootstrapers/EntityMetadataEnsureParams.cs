using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ShardingCore.Core.EntityMetadatas;

/*
* @Author: xjm
* @Description:
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrapers
{
    /// <summary>
    /// 分表、分库对象确定参数用来初始化<see cref="EntityMetadata"/>时所需的信息
    /// </summary>
    public class EntityMetadataEnsureParams
    {
        public EntityMetadataEnsureParams(IEntityType entityType)
        {
            EntityType = entityType;

#if !EFCORE2
            var virtualTableName = entityType.GetTableName();
#endif
#if EFCORE2
            var virtualTableName = entityType.Relational().TableName;
#endif
            VirtualTableName = virtualTableName;
        }

        public IEntityType EntityType { get; }
        public string VirtualTableName { get; }
    }
}
