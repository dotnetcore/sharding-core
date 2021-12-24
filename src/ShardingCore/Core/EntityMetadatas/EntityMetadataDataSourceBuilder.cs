using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Internal;
#endif

namespace ShardingCore.Core.EntityMetadatas
{
    public class EntityMetadataDataSourceBuilder<TEntity> where TEntity : class
    {
        private readonly EntityMetadata _entityMetadata;

        private EntityMetadataDataSourceBuilder(EntityMetadata entityMetadata)
        {
            _entityMetadata = entityMetadata;
        }
        /// <summary>
        /// 设置分表字段  表达式
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public EntityMetadataDataSourceBuilder<TEntity> ShardingProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var propertyAccess = propertyExpression.GetPropertyAccess();
            _entityMetadata.SetShardingDataSourceProperty(propertyAccess);
            return this;
        }
        /// <summary>
        /// 设置分表字段 属性名称
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public EntityMetadataDataSourceBuilder<TEntity> ShardingProperty(string propertyName)
        {
            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            _entityMetadata.SetShardingDataSourceProperty(propertyInfo);
            return this;
        }
        public EntityMetadataDataSourceBuilder<TEntity> ShardingExtraProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var propertyAccess = propertyExpression.GetPropertyAccess();
            _entityMetadata.AddExtraSharingDataSourceProperty(propertyAccess);
            return this;
        }
        public EntityMetadataDataSourceBuilder<TEntity> ShardingExtraProperty(string propertyName)
        {
            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            _entityMetadata.AddExtraSharingDataSourceProperty(propertyInfo);
            return this;
        }
        /// <summary>
        /// 是否启动建表
        /// </summary>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        public EntityMetadataDataSourceBuilder<TEntity> AutoCreateDataSource(bool? autoCreate)
        {
            _entityMetadata.AutoCreateDataSourceTable = autoCreate;
            return this;
        }

        /// <summary>
        /// 创建分库对象元数据创建者
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <returns></returns>
        public static EntityMetadataDataSourceBuilder<TEntity> CreateEntityMetadataDataSourceBuilder(EntityMetadata entityMetadata)
        {
            return new EntityMetadataDataSourceBuilder<TEntity>(entityMetadata);
        }

    }
}
