using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Internal;
#endif

namespace ShardingCore.Core.EntityMetadatas
{
    /// <summary>
    /// 分表对象元数据建造者
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
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
        public EntityMetadataTableBuilder<TEntity> ShardingExtraProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var propertyAccess = propertyExpression.GetPropertyAccess();
            _entityMetadata.AddExtraSharingTableProperty(propertyAccess);
            return this;
        }
        public EntityMetadataTableBuilder<TEntity> ShardingExtraProperty(string propertyName)
        {
            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            _entityMetadata.AddExtraSharingTableProperty(propertyInfo);
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
        /// 是否启动的时候创建表，仅在启动时判断该属性，如果你是按时间分表的那么这个属性将不会在特定时间创建对应的表信息需要手动进行表创建和添加
        /// 当然您也可以使用系统默认的时间路由，通过重写AutoCreateByTime方法重写返回true来保证sharding_core开启定时任务并且已经集成自动创建表和库
        /// </summary>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        public EntityMetadataTableBuilder<TEntity> AutoCreateTable(bool? autoCreate)
        {
            _entityMetadata.AutoCreateTable = autoCreate;
            return this;
        }
        /// <summary>
        /// 创建分表元数据建造者
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <returns></returns>
        public static EntityMetadataTableBuilder<TEntity> CreateEntityMetadataTableBuilder(EntityMetadata entityMetadata)
        {
            return new EntityMetadataTableBuilder<TEntity>(entityMetadata);
        }

    }
}
