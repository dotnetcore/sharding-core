using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;
using ShardingCore.Sharding.PaginationConfigurations;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.EntityQueryConfigurations;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 18 December 2020 14:33:01
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractVirtualTableRoute<T, TKey> : IVirtualTableRoute<T>, IEntityMetadataAutoBindInitializer where T : class
    {

        private readonly DoOnlyOnce _doOnlyOnce = new DoOnlyOnce();
        // public IShardingRouteConfigOptions RouteConfigOptions { get; private set; }

        public PaginationMetadata PaginationMetadata { get; private set; }
        public bool EnablePagination =>PaginationMetadata != null;
        public EntityQueryMetadata EntityQueryMetadata { get;  private set; }
        public bool EnableEntityQuery =>EntityQueryMetadata != null;
        public IShardingProvider RouteShardingProvider { get;  private set;}

        public virtual void Initialize(EntityMetadata entityMetadata,IShardingProvider shardingProvider)
        {
            if (!_doOnlyOnce.IsUnDo())
                throw new ShardingCoreInvalidOperationException("already init");
            RouteShardingProvider = shardingProvider;
            EntityMetadata = entityMetadata;
            // RouteConfigOptions =shardingProvider.GetService<IShardingRouteConfigOptions>();
            var paginationConfiguration = CreatePaginationConfiguration();
             if (paginationConfiguration!=null)
             {
                 PaginationMetadata = new PaginationMetadata();
                 var paginationBuilder = new PaginationBuilder<T>(PaginationMetadata);
                 paginationConfiguration.Configure(paginationBuilder);
             }

             var entityQueryConfiguration = CreateEntityQueryConfiguration();
             if (entityQueryConfiguration != null)
             {
                 EntityQueryMetadata = new EntityQueryMetadata();
                 var entityQueryBuilder = new EntityQueryBuilder<T>(EntityQueryMetadata);
                 entityQueryConfiguration.Configure(entityQueryBuilder);
             }
        }


        public EntityMetadata EntityMetadata { get; private set; }

        /// <summary>
        /// 如何将分表字段转成后缀
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public abstract string ShardingKeyToTail(object shardingKey);

        /// <summary>
        /// 根据表达式路由
        /// </summary>
        /// <param name="dataSourceRouteResult"></param>
        /// <param name="queryable"></param>
        /// <param name="isQuery"></param>
        /// <returns></returns>
        public abstract List<TableRouteUnit> RouteWithPredicate(DataSourceRouteResult dataSourceRouteResult, IQueryable queryable,bool isQuery);
        
        /// <summary>
        /// 根据值路由
        /// </summary>
        /// <param name="dataSourceRouteResult"></param>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public abstract TableRouteUnit RouteWithValue(DataSourceRouteResult dataSourceRouteResult, object shardingKey);
        /// <summary>
        /// 返回数据库现有的尾巴
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetTails();

        /// <summary>
        /// 配置分表的一些信息
        /// 1.ShardingProperty 哪个字段分表
        /// 2.TableSeparator 分表的后缀和表名的连接符
        /// 3.AutoCreateTable 启动时是否需要创建对应的分表信息
        /// </summary>
        /// <param name="builder"></param>
        public abstract void Configure(EntityMetadataTableBuilder<T> builder);

        /// <summary>
        /// 创建分页配置
        /// </summary>
        /// <returns></returns>
        public virtual IPaginationConfiguration<T> CreatePaginationConfiguration()
        {
            return null;
        }
        /// <summary>
        /// 创建熟悉怒查询配置
        /// </summary>
        /// <returns></returns>
        public virtual IEntityQueryConfiguration<T> CreateEntityQueryConfiguration()
        {
            return null;
        }
    }
}