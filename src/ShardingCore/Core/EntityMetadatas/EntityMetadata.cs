using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 分表或者分库对象的元数据信息记录对象在ShardingCore框架下的一些简单的信息
    /// </summary>
    public class EntityMetadata
    {private const string QueryFilter = "QueryFilter";
        public EntityMetadata(Type entityType)
        {
            EntityType = entityType;
            ShardingDataSourceProperties = new Dictionary<string, PropertyInfo>();
            ShardingTableProperties = new Dictionary<string, PropertyInfo>();
        }
        /// <summary>
        /// 分表类型 sharding entity type
        /// </summary>
        public Type EntityType { get; }

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
        /// 分库所有字段包括 ShardingDataSourceProperty
        /// </summary>
        public IDictionary<string, PropertyInfo> ShardingDataSourceProperties { get; }

        /// <summary>
        /// 启动时是否建表 auto create data source when start app
        /// </summary>
        public bool? AutoCreateDataSourceTable { get;  set; }

        /// <summary>
        /// 分表字段 sharding table property
        /// </summary>
        public PropertyInfo ShardingTableProperty { get; private set; }
        /// <summary>
        /// 分表所有字段包括 ShardingTableProperty
        /// </summary>
        public IDictionary<string, PropertyInfo> ShardingTableProperties { get;}


        /// <summary>
        /// 启动时是否建表 auto create table when start app
        /// </summary>
        public bool? AutoCreateTable { get;  set; }

        /// <summary>
        /// 分表隔离器 table sharding tail prefix
        /// </summary>
        public string TableSeparator { get; private set; } = "_";
        
        /// <summary>
        /// 逻辑表名
        /// </summary>
        public string LogicTableName { get; private set; }
        
        /// <summary>
        /// 主键
        /// </summary>
        public IReadOnlyList<PropertyInfo> PrimaryKeyProperties { get; private set; }
        /**
         * efcore query filter
         */
        public LambdaExpression QueryFilterExpression { get; private set; }

        /// <summary>
        /// 是否单主键
        /// </summary>
        public bool IsSingleKey { get; private set; }

        public void SetEntityModel(IEntityType dbEntityType)
        {
            LogicTableName = dbEntityType.GetEntityTypeTableName();
            QueryFilterExpression= dbEntityType.GetAnnotations().FirstOrDefault(o=>o.Name== QueryFilter)?.Value as LambdaExpression;
            PrimaryKeyProperties = dbEntityType.FindPrimaryKey()?.Properties?.Select(o => o.PropertyInfo)?.ToList() ??
                                   new List<PropertyInfo>();
            IsSingleKey=PrimaryKeyProperties.Count == 1;
        }
        
        
        
        /// <summary>
        /// 设置分库字段
        /// </summary>
        /// <param name="propertyInfo"></param>
        public void SetShardingDataSourceProperty(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));
            if (ShardingDataSourceProperties.ContainsKey(propertyInfo.Name))
                throw new ShardingCoreConfigException($"same sharding data source property name:[{propertyInfo.Name}] don't repeat add");
            ShardingDataSourceProperty = propertyInfo;
            ShardingDataSourceProperties.Add(propertyInfo.Name, propertyInfo);
        }
        /// <summary>
        /// 添加额外分表字段
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <exception cref="ShardingCoreConfigException"></exception>
        public void AddExtraSharingDataSourceProperty(PropertyInfo propertyInfo)
        {
            if (ShardingDataSourceProperties.ContainsKey(propertyInfo.Name))
                throw new ShardingCoreConfigException($"same sharding data source property name:[{propertyInfo.Name}] don't repeat add");
            ShardingDataSourceProperties.Add(propertyInfo.Name, propertyInfo);
        }
        /// <summary>
        /// 设置分表字段
        /// </summary>
        /// <param name="propertyInfo"></param>
        public void SetShardingTableProperty(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));
            if (ShardingTableProperties.ContainsKey(propertyInfo.Name))
                throw new ShardingCoreConfigException($"same sharding table property name:[{propertyInfo.Name}] don't repeat add");
            ShardingTableProperty = propertyInfo;
            ShardingTableProperties.Add(propertyInfo.Name, propertyInfo);
        }
        /// <summary>
        /// 添加额外分表字段
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <exception cref="ShardingCoreConfigException"></exception>
        public void AddExtraSharingTableProperty(PropertyInfo propertyInfo)
        {
            if (ShardingTableProperties.ContainsKey(propertyInfo.Name))
                throw new ShardingCoreConfigException($"same sharding table property name:[{propertyInfo.Name}] don't repeat add");
            ShardingTableProperties.Add(propertyInfo.Name, propertyInfo);
        }

        /// <summary>
        /// 分表表和后缀连接器
        /// </summary>
        /// <param name="separator"></param>
        public void SetTableSeparator(string separator)
        {
            TableSeparator = separator;
        }
        /// <summary>
        /// 启动时检查分库信息是否完整
        /// </summary>
        public void CheckShardingDataSourceMetadata()
        {
            if (!IsMultiDataSourceMapping)
            {
                throw new ShardingCoreException($"not found  entity:{EntityType} configure");
            }
            if(ShardingDataSourceProperty==null)
            {
                throw new ShardingCoreException($"not found  entity:{EntityType} configure sharding property");
            }
        }
        /// <summary>
        /// 启动时检查分表信息是否完整
        /// </summary>
        public void CheckShardingTableMetadata()
        {
            if (!IsMultiTableMapping)
            {
                throw new ShardingCoreException($"not found  entity:{EntityType} configure");
            }
            if (ShardingTableProperty == null)
            {
                throw new ShardingCoreException($"not found  entity:{EntityType} configure sharding property");
            }
        }
        /// <summary>
        /// 启动时检查对象信息是否完整
        /// </summary>
        public void CheckGenericMetadata()
        {
            if (null == EntityType ||
                (!IsMultiTableMapping && !IsMultiDataSourceMapping))
            {
                throw new ShardingCoreException($"not found  entity:{EntityType} configure");
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
