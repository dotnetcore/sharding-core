using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Exceptions;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    public class EntityQueryBuilder<TEntity> where TEntity : class
    {
        private readonly EntityQueryMetadata _entityQueryMetadata;

        public EntityQueryBuilder(EntityQueryMetadata entityQueryMetadata)
        {
            _entityQueryMetadata = entityQueryMetadata;
        }
        /// <summary>
        /// 添加条件顺序查询配置
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="primaryOrderPropertyExpression">主排序字段</param>
        /// <param name="parallelThreadQueryCount">迭代器获取该值不生效</param>
        /// <param name="routeComparer"></param>
        /// <returns></returns>
        public EntityQueryBuilder<TEntity> AddEntityQuerySeqConfig<TProperty>(Expression<Func<TEntity, TProperty>> primaryOrderPropertyExpression, int parallelThreadQueryCount, IComparer<string> routeComparer = null)
        {
            var entitySeqQueryConfig = new EntitySeqQueryConfig(primaryOrderPropertyExpression, parallelThreadQueryCount, routeComparer);
            if (_entityQueryMetadata.EntityOrderSeqQueryConfigs.ContainsKey(entitySeqQueryConfig.PrimaryOrderPropertyInfo.Name))
            {
                throw new ShardingCoreConfigException(
                    $"repeat {nameof(AddEntityQuerySeqConfig)} property name:[{entitySeqQueryConfig.PrimaryOrderPropertyInfo.Name}]");
            }
            _entityQueryMetadata.EntityOrderSeqQueryConfigs.Add(entitySeqQueryConfig.PrimaryOrderPropertyInfo.Name, entitySeqQueryConfig);
            return this;
        }
    }
}
