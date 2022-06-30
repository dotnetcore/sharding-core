using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.EntityShardingMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Jobs;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Logger;
using ShardingCore.Sharding.Abstractions;

/*
* @Author: xjm
* @Description:
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrappers
{
    /// <summary>
    /// 对象元数据初始化器
    /// </summary>
    /// <typeparam name="TShardingDbContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityMetadataInitializer<TShardingDbContext,TEntity>: IEntityMetadataInitializer where TShardingDbContext:DbContext,IShardingDbContext where TEntity:class
    {
        private static readonly ILogger<EntityMetadataInitializer<TShardingDbContext, TEntity>> _logger=InternalLoggerFactory.CreateLogger<EntityMetadataInitializer<TShardingDbContext,TEntity>>();
        // private const string QueryFilter = "QueryFilter";
        // private readonly IEntityType _entityType;
        // private readonly string _virtualTableName;
        // private readonly Expression<Func<TEntity,bool>> _queryFilterExpression;
        private readonly Type _shardingEntityType;
        private readonly IShardingEntityConfigOptions _shardingEntityConfigOptions;
        private readonly IVirtualDataSourceRouteManager _virtualDataSourceRouteManager;
        private readonly ITableRouteManager _tableRouteManager;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IJobManager _jobManager;

        public EntityMetadataInitializer(
            IShardingEntityConfigOptions shardingEntityConfigOptions,
            IVirtualDataSourceRouteManager virtualDataSourceRouteManager,
            ITableRouteManager tableRouteManager,
            IEntityMetadataManager entityMetadataManager,
            IJobManager jobManager
            )
        {
            _shardingEntityType = typeof(TEntity);
            // _entityType = entityMetadataEnsureParams.EntityType;
            // _virtualTableName = entityMetadataEnsureParams.VirtualTableName;
            // _queryFilterExpression = entityMetadataEnsureParams.EntityType.GetAnnotations().FirstOrDefault(o=>o.Name== QueryFilter)?.Value as Expression<Func<TEntity, bool>>;
            _shardingEntityConfigOptions = shardingEntityConfigOptions;
            _virtualDataSourceRouteManager = virtualDataSourceRouteManager;
            _tableRouteManager = tableRouteManager;
            _entityMetadataManager = entityMetadataManager;
            _jobManager = jobManager;
        }
        /// <summary>
        /// 初始化
        /// 针对对象在dbcontext中的主键获取
        /// 并且针对分库下的特性加接口的支持，然后是分库路由的配置覆盖
        /// 分表下的特性加接口的支持，然后是分表下的路由的配置覆盖
        /// </summary>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        public void Initialize()
        {
            var entityMetadata = new EntityMetadata(_shardingEntityType, typeof(TShardingDbContext));
            if (!_entityMetadataManager.AddEntityMetadata(entityMetadata))
                throw new ShardingCoreInvalidOperationException($"repeat add entity metadata {_shardingEntityType.FullName}");
            //设置标签
            if (_shardingEntityConfigOptions.TryGetVirtualDataSourceRoute<TEntity>(out var virtualDataSourceRouteType))
            {
                var creatEntityMetadataDataSourceBuilder = EntityMetadataDataSourceBuilder<TEntity>.CreateEntityMetadataDataSourceBuilder(entityMetadata);
                //配置属性分库信息
                EntityMetadataHelper.Configure(creatEntityMetadataDataSourceBuilder);
                var dataSourceRoute = CreateVirtualDataSourceRoute(virtualDataSourceRouteType);
                if (dataSourceRoute is IEntityMetadataAutoBindInitializer entityMetadataAutoBindInitializer)
                {
                    entityMetadataAutoBindInitializer.Initialize(entityMetadata);
                }
                //配置分库信息
                if(dataSourceRoute is IEntityMetadataDataSourceConfiguration<TEntity> entityMetadataDataSourceConfiguration)
                {
                    entityMetadataDataSourceConfiguration.Configure(creatEntityMetadataDataSourceBuilder);
                }
                _virtualDataSourceRouteManager.AddVirtualDataSourceRoute(dataSourceRoute);
                entityMetadata.CheckShardingDataSourceMetadata();

            }
            if (_shardingEntityConfigOptions.TryGetVirtualTableRoute<TEntity>(out var virtualTableRouteType))
            {
                if (!typeof(TShardingDbContext).IsShardingTableDbContext())
                    throw new ShardingCoreInvalidOperationException(
                        $"{typeof(TShardingDbContext)} is not impl {nameof(IShardingTableDbContext)},not support sharding table");
                var entityMetadataTableBuilder = EntityMetadataTableBuilder<TEntity>.CreateEntityMetadataTableBuilder(entityMetadata);
                //配置属性分表信息
                EntityMetadataHelper.Configure(entityMetadataTableBuilder);

                var virtualTableRoute = CreateVirtualTableRoute(virtualTableRouteType);
                if (virtualTableRoute is IEntityMetadataAutoBindInitializer entityMetadataAutoBindInitializer)
                {
                    entityMetadataAutoBindInitializer.Initialize(entityMetadata);
                }
                //配置分表信息
                if (virtualTableRoute is IEntityMetadataTableConfiguration<TEntity> createEntityMetadataTableConfiguration)
                {
                    createEntityMetadataTableConfiguration.Configure(entityMetadataTableBuilder);
                }
                //创建虚拟表
                _tableRouteManager.AddRoute(virtualTableRoute);
                //检测校验分表分库对象元数据
                entityMetadata.CheckShardingTableMetadata();
                //添加任务
                if (virtualTableRoute is IJob routeJob && routeJob.AutoCreateTableByTime())
                {
                    var jobEntry = JobEntryFactory.Create(routeJob);
                    _jobManager.AddJob(jobEntry);
                }
            }
            entityMetadata.CheckGenericMetadata();
        }

        private IVirtualDataSourceRoute<TEntity> CreateVirtualDataSourceRoute(Type virtualRouteType)
        {
            var instance = ShardingRuntimeContext.GetInstance().CreateInstance(virtualRouteType);
            return (IVirtualDataSourceRoute<TEntity>)instance;
        }


        private IVirtualTableRoute<TEntity> CreateVirtualTableRoute(Type virtualRouteType)
        {
            var instance = ShardingRuntimeContext.GetInstance().CreateInstance(virtualRouteType);
            return (IVirtualTableRoute<TEntity>)instance;
        }
    }
}
