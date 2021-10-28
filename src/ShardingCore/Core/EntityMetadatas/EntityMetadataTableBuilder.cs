using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Internal;
#endif

namespace ShardingCore.Core.EntityMetadatas
{
    public class EntityMetadataTableBuilder<TEntity> where TEntity : class
    {
        private readonly EntityMetadata _entityMetadata;

        private EntityMetadataTableBuilder(EntityMetadata entityMetadata)
        {
            _entityMetadata = entityMetadata;
        }
        /// <summary>
        /// 设置分表字段
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public EntityMetadataTableBuilder<TEntity> ShardingProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var propertyAccess = propertyExpression.GetPropertyAccess();
            _entityMetadata.SetShardingTableProperty(propertyAccess);
            return this;
        }
        public EntityMetadataTableBuilder<TEntity> ShardingProperty(string propertyName)
        {
            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            _entityMetadata.SetShardingTableProperty(propertyInfo);
            return this;
        }
        /// <summary>
        /// 分表表和后缀连接器
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public EntityMetadataTableBuilder<TEntity> TableSeparator(string separator)
        {
            _entityMetadata.SetTableSeparator(separator);
            return this;
        }
        /// <summary>
        /// 是否启动建表
        /// </summary>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        public EntityMetadataTableBuilder<TEntity> AutoCreateTable(bool? autoCreate)
        {
            _entityMetadata.AutoCreateTable = autoCreate;
            return this;
        }

        public static EntityMetadataTableBuilder<TEntity> CreateEntityMetadataTableBuilder(EntityMetadata entityMetadata)
        {
            return new EntityMetadataTableBuilder<TEntity>(entityMetadata);
        }

    }
}
