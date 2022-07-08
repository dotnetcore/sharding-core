using System;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.EntityShardingMetadatas;
using ShardingCore.Core.ServiceProviders;
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
    /// <typeparam name="TEntity"></typeparam>
    public class EntityMetadataInitializer<TEntity>: IEntityMetadataInitializer where TEntity:class
    {
        private readonly Type _shardingEntityType;
        private readonly IShardingProvider _shardingProvider;
        private readonly IShardingRouteConfigOptions _shardingRouteConfigOptions;
        private readonly IDataSourceRouteManager _dataSourceRouteManager;
        private readonly ITableRouteManager _tableRouteManager;
        private readonly IEntityMetadataManager _entityMetadataManager;

        public EntityMetadataInitializer(
            IShardingProvider shardingProvider,
            IShardingRouteConfigOptions shardingRouteConfigOptions,
            IDataSourceRouteManager dataSourceRouteManager,
            ITableRouteManager tableRouteManager,
            IEntityMetadataManager entityMetadataManager
            )
        {
            _shardingEntityType = typeof(TEntity);
            _shardingProvider = shardingProvider;
            _shardingRouteConfigOptions = shardingRouteConfigOptions;
            _dataSourceRouteManager = dataSourceRouteManager;
            _tableRouteManager = tableRouteManager;
            _entityMetadataManager = entityMetadataManager;
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
            var entityMetadata = new EntityMetadata(_shardingEntityType);
            if (!_entityMetadataManager.AddEntityMetadata(entityMetadata))
                throw new ShardingCoreInvalidOperationException($"repeat add entity metadata {_shardingEntityType.FullName}");
            //设置标签
            if (_shardingRouteConfigOptions.TryGetVirtualDataSourceRoute<TEntity>(out var virtualDataSourceRouteType))
            {
                var creatEntityMetadataDataSourceBuilder = EntityMetadataDataSourceBuilder<TEntity>.CreateEntityMetadataDataSourceBuilder(entityMetadata);
                //配置属性分库信息
                var dataSourceRoute = CreateVirtualDataSourceRoute(virtualDataSourceRouteType);
                if (dataSourceRoute is IEntityMetadataAutoBindInitializer entityMetadataAutoBindInitializer)
                {
                    entityMetadataAutoBindInitializer.Initialize(entityMetadata,_shardingProvider);
                }
                //配置分库信息
                if(dataSourceRoute is IEntityMetadataDataSourceConfiguration<TEntity> entityMetadataDataSourceConfiguration)
                {
                    entityMetadataDataSourceConfiguration.Configure(creatEntityMetadataDataSourceBuilder);
                }
                _dataSourceRouteManager.AddDataSourceRoute(dataSourceRoute);
                entityMetadata.CheckShardingDataSourceMetadata();

            }
            if (_shardingRouteConfigOptions.TryGetVirtualTableRoute<TEntity>(out var virtualTableRouteType))
            {
                var entityMetadataTableBuilder = EntityMetadataTableBuilder<TEntity>.CreateEntityMetadataTableBuilder(entityMetadata);
                //配置属性分表信息

                var virtualTableRoute = CreateVirtualTableRoute(virtualTableRouteType);
                if (virtualTableRoute is IEntityMetadataAutoBindInitializer entityMetadataAutoBindInitializer)
                {
                    entityMetadataAutoBindInitializer.Initialize(entityMetadata,_shardingProvider);
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
                if (virtualTableRoute is IJob routeJob&&routeJob.AppendJob())
                {
                    var jobEntry = JobEntryFactory.Create(routeJob);
                    _shardingProvider.GetRequiredService<IJobManager>().AddJob(jobEntry);
                }
            }
            entityMetadata.CheckGenericMetadata();
        }

        private IVirtualDataSourceRoute<TEntity> CreateVirtualDataSourceRoute(Type virtualRouteType)
        {
            var instance = _shardingProvider.CreateInstance(virtualRouteType);
            return (IVirtualDataSourceRoute<TEntity>)instance;
        }


        private IVirtualTableRoute<TEntity> CreateVirtualTableRoute(Type virtualRouteType)
        {
            var instance = _shardingProvider.CreateInstance(virtualRouteType);
            return (IVirtualTableRoute<TEntity>)instance;
        }
    }
}
