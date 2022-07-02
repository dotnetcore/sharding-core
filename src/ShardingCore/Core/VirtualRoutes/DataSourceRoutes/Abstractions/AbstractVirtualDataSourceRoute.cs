using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.PaginationConfigurations;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Extensions;
using ShardingCore.Sharding.EntityQueryConfigurations;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 18 December 2020 14:33:01
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractVirtualDataSourceRoute<TEntity, TKey> : IVirtualDataSourceRoute<TEntity>, IEntityMetadataAutoBindInitializer where TEntity : class
    {
        public EntityMetadata EntityMetadata { get; private set; }
        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();
        // public IShardingRouteConfigOptions RouteConfigOptions { get; private set; }

        public new PaginationMetadata PaginationMetadata { get; protected set; }
        public bool EnablePagination => PaginationMetadata != null;

        //public new EntityQueryMetadata EntityQueryMetadata { get; protected set; }
        //public bool EnableEntityQuery => EnableEntityQuery != null;

        public IShardingProvider RouteShardingProvider { get; private set;}

        public void Initialize(EntityMetadata entityMetadata,IShardingProvider shardingProvider)
        {
            if (!_doOnlyOnce.IsUnDo())
                throw new ShardingCoreInvalidOperationException("already init");
            RouteShardingProvider = shardingProvider;
            EntityMetadata = entityMetadata;

            // RouteConfigOptions = shardingProvider.GetService<IShardingRouteConfigOptions>();
            var paginationConfiguration = CreatePaginationConfiguration();
            if (paginationConfiguration != null)
            {
                PaginationMetadata = new PaginationMetadata();
                var paginationBuilder = new PaginationBuilder<TEntity>(PaginationMetadata);
                paginationConfiguration.Configure(paginationBuilder);
            }
            // var entityQueryConfiguration = CreateEntityQueryConfiguration();
            // if (entityQueryConfiguration != null)
            // {
            //     EntityQueryMetadata = new EntityQueryMetadata();
            //     var entityQueryBuilder= new EntityQueryBuilder<TEntity>(EntityQueryMetadata);
            //     entityQueryConfiguration.Configure(entityQueryBuilder);
            // }
        }
        public virtual IPaginationConfiguration<TEntity> CreatePaginationConfiguration()
        {
            return null;
        }

        //public virtual IEntityQueryConfiguration<TEntity> CreateEntityQueryConfiguration()
        //{
        //    return null;
        //}


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
        /// <summary>
        /// 配置分库的一些信息
        /// 1.ShardingProperty 哪个字段分库
        /// 2.AutoCreateDataSource 启动时是否需要创建数据源
        /// </summary>
        /// <param name="builder"></param>
        public abstract void Configure(EntityMetadataDataSourceBuilder<TEntity> builder);
    }
}