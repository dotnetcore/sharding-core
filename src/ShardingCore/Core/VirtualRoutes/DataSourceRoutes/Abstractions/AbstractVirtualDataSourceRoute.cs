using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.EntityShardingMetadatas;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.PaginationConfigurations;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 18 December 2020 14:33:01
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractVirtualDataSourceRoute<T, TKey> : IVirtualDataSourceRoute<T>, IEntityMetadataAutoBindInitializer where T : class
    {
        public EntityMetadata EntityMetadata { get; private set; }
        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();


        public void Initialize(EntityMetadata entityMetadata)
        {
            if (!_doOnlyOnce.IsUnDo())
                throw new InvalidOperationException("already init");
            EntityMetadata = entityMetadata;
            var paginationConfiguration = CreatePaginationConfiguration();
            if (paginationConfiguration != null)
            {
                PaginationMetadata = new PaginationMetadata();
                var paginationBuilder = new PaginationBuilder<T>(PaginationMetadata);
                paginationConfiguration.Configure(paginationBuilder);
            }

        }
        public virtual IPaginationConfiguration<T> CreatePaginationConfiguration()
        {
            return null;
        }

        public virtual IEntityMetadataDataSourceConfiguration<T> CreateEntityMetadataDataSourceConfiguration()
        {
            return null;
        }

        public new PaginationMetadata PaginationMetadata { get; protected set; }
        public bool EnablePagination => PaginationMetadata != null;

        /// <summary>
        /// 分库字段object类型的如何转成对应的泛型类型how convert sharding key to generic type key value
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        protected abstract TKey ConvertToShardingKey(object shardingKey);
        /// <summary>
        /// 分库字段如何转成对应的数据源名称 how  convert sharding data source key to data source name
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public abstract string ShardingKeyToDataSourceName(object shardingKey);

        /// <summary>
        /// 根据表达式返回对应的数据源名称 find data source names with queryable
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="isQuery"></param>
        /// <returns></returns>
        public abstract List<string> RouteWithPredicate(IQueryable queryable, bool isQuery);
        /// <summary>
        /// 值如何转成对应的数据源
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public abstract string RouteWithValue(object shardingKey);

        public abstract List<string> GetAllDataSourceNames();
        public abstract bool AddDataSourceName(string dataSourceName);
        public void Configure(EntityMetadataDataSourceBuilder<T> builder)
        {
            
        }
    }
}