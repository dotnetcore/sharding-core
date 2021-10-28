using System;
using System.Collections.Generic;
using System.Reflection;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.EntityMetadatas
{
    public class EntityMetadata
    {
        public EntityMetadata(Type entityType, string virtualTableName, IReadOnlyList<PropertyInfo> primaryKeyProperties)
        {
            EntityType = entityType;
            VirtualTableName = virtualTableName;
            PrimaryKeyProperties = primaryKeyProperties;
            IsSingleKey= PrimaryKeyProperties.Count == 1;
        }
        /// <summary>
        /// 分表类型 sharding entity type
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// 分表的原表名 original table name in db exclude tail
        /// </summary>
        public string VirtualTableName { get; }
        /// <summary>
        /// 主键
        /// </summary>
        public IReadOnlyList<PropertyInfo> PrimaryKeyProperties { get; }
        /// <summary>
        /// 是否单主键
        /// </summary>
        public bool IsSingleKey { get; }

        /// <summary>
        /// 是否多数据源
        /// </summary>
        public bool IsMultiDataSourceMapping => null != ShardingDataSourceProperty;

        /// <summary>
        /// 是否分表
        /// </summary>
        public bool IsMultiTableMapping => null != ShardingTableProperty;
        /// <summary>
        /// 分库字段
        /// </summary>
        public PropertyInfo ShardingDataSourceProperty { get; private set; }

        /// <summary>
        /// 启动时是否建表 auto create data source when start app
        /// </summary>
        public bool? AutoCreateDataSourceTable { get;  set; }

        /// <summary>
        /// 分表字段 sharding table property
        /// </summary>
        public PropertyInfo ShardingTableProperty { get; private set; }


        /// <summary>
        /// 启动时是否建表 auto create table when start app
        /// </summary>
        public bool? AutoCreateTable { get;  set; }

        /// <summary>
        /// 分表隔离器 table sharding tail prefix
        /// </summary>
        public string TableSeparator { get; private set; } = "_";
        /// <summary>
        /// 设置分库字段
        /// </summary>
        /// <param name="propertyInfo"></param>
        public void SetShardingDataSourceProperty(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));
            Check.ShouldNull(ShardingDataSourceProperty, nameof(ShardingDataSourceProperty));
            ShardingDataSourceProperty = propertyInfo;
        }
        /// <summary>
        /// 设置分表字段
        /// </summary>
        /// <param name="propertyInfo"></param>
        public void SetShardingTableProperty(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));
            ShardingTableProperty = propertyInfo;
        }

        /// <summary>
        /// 分表表和后缀连接器
        /// </summary>
        /// <param name="separator"></param>
        public void SetTableSeparator(string separator)
        {
            TableSeparator = separator;
        }

        public void CheckMetadata()
        {
            if (null == EntityType || null == PrimaryKeyProperties || null == VirtualTableName ||
                (!IsMultiTableMapping && !IsMultiDataSourceMapping))
            {
                throw new ShardingCoreException($"configure entity:{EntityType}");
            }
        }
        protected bool Equals(EntityMetadata other)
        {
            return Equals(EntityType, other.EntityType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityMetadata)obj);
        }

        public override int GetHashCode()
        {
            return (EntityType != null ? EntityType.GetHashCode() : 0);
        }
    }
}
