using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.EntityShardingMetadatas;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
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
        private const string QueryFilter = "QueryFilter";
        private readonly IEntityType _entityType;
        private readonly string _virtualTableName;
        private readonly Expression<Func<TEntity,bool>> _queryFilterExpression;
        private readonly IShardingEntityConfigOptions<TShardingDbContext> _shardingEntityConfigOptions;
        private readonly IVirtualDataSourceManager<TShardingDbContext> _virtualDataSourceManager;
        private readonly IVirtualDataSourceRouteManager<TShardingDbContext> _virtualDataSourceRouteManager;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;

        public EntityMetadataInitializer(EntityMetadataEnsureParams entityMetadataEnsureParams,
            IShardingEntityConfigOptions<TShardingDbContext> shardingEntityConfigOptions,
            IVirtualDataSourceManager<TShardingDbContext> virtualDataSourceManager,
            IVirtualDataSourceRouteManager<TShardingDbContext> virtualDataSourceRouteManager,
            IVirtualTableManager<TShardingDbContext> virtualTableManager,
            IEntityMetadataManager<TShardingDbContext> entityMetadataManager
            )
        {
            _entityType = entityMetadataEnsureParams.EntityType;
            _virtualTableName = entityMetadataEnsureParams.VirtualTableName;
            _queryFilterExpression = entityMetadataEnsureParams.EntityType.GetAnnotations().FirstOrDefault(o=>o.Name== QueryFilter)?.Value as Expression<Func<TEntity, bool>>;
            _shardingEntityConfigOptions = shardingEntityConfigOptions;
            _virtualDataSourceManager = virtualDataSourceManager;
            _virtualDataSourceRouteManager = virtualDataSourceRouteManager;
            _virtualTableManager = virtualTableManager;
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
            var shardingEntityType = _entityType.ClrType;
            var entityMetadata = new EntityMetadata(shardingEntityType, _virtualTableName,typeof(TShardingDbContext),_entityType.FindPrimaryKey()?.Properties?.Select(o=>o.PropertyInfo)?.ToList()??new List<PropertyInfo>(),_queryFilterExpression);
            if (!_entityMetadataManager.AddEntityMetadata(entityMetadata))
                throw new ShardingCoreInvalidOperationException($"repeat add entity metadata {shardingEntityType.FullName}");
            //设置标签
            if (_shardingEntityConfigOptions.TryGetVirtualDataSourceRoute<TEntity>(out var virtualDataSourceRouteType))
            {
                var creatEntityMetadataDataSourceBuilder = EntityMetadataDataSourceBuilder<TEntity>.CreateEntityMetadataDataSourceBuilder(entityMetadata);
                //配置属性分库信息
                EntityMetadataHelper.Configure(creatEntityMetadataDataSourceBuilder);
                var dataSourceRoute = CreateVirtualDataSourceRoute(virtualDataSourceRouteType, entityMetadata);
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

                var virtualTableRoute = CreateVirtualTableRoute(virtualTableRouteType, entityMetadata);
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
                var virtualTable = CreateVirtualTable(virtualTableRoute,entityMetadata);
                _virtualTableManager.AddVirtualTable(virtualTable);
                //检测校验分表分库对象元数据
                entityMetadata.CheckShardingTableMetadata();
                //添加任务
                if (virtualTableRoute is IJob routeJob && routeJob.AutoCreateTableByTime())
                {
                    var jobManager = ShardingContainer.GetService<IJobManager>();
                    var jobEntry = JobEntryFactory.Create(routeJob);
                    jobManager.AddJob(jobEntry);
                }
            }
            entityMetadata.CheckGenericMetadata();
        }

        private IVirtualDataSourceRoute<TEntity> CreateVirtualDataSourceRoute(Type virtualRouteType,EntityMetadata entityMetadata)
        {
            var constructors
                = virtualRouteType.GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();
            if (constructors.Length != 1)
            {
                throw new ArgumentException(
                    $"virtual route :[{virtualRouteType}] found more  declared constructor ");
            }

            var @params = constructors[0].GetParameters().Select(x => ShardingContainer.GetService(x.ParameterType))
                .ToArray();
            object o = Activator.CreateInstance(virtualRouteType, @params);
            return (IVirtualDataSourceRoute<TEntity>)o;
        }


        private IVirtualTableRoute<TEntity> CreateVirtualTableRoute(Type virtualRouteType, EntityMetadata entityMetadata)
        {
            var constructors
                = virtualRouteType.GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();

            if (constructors.Length !=1)
            {
                throw new ArgumentException(
                    $"virtual route :[{virtualRouteType}] found more  declared constructor ");
            }
            var @params = constructors[0].GetParameters().Select(x => ShardingContainer.GetService(x.ParameterType))
                .ToArray();
            object o = Activator.CreateInstance(virtualRouteType, @params);
            return (IVirtualTableRoute<TEntity>)o;
        }

        private IVirtualTable<TEntity> CreateVirtualTable(IVirtualTableRoute<TEntity> virtualTableRoute,EntityMetadata entityMetadata)
        {
            return new DefaultVirtualTable<TEntity>(virtualTableRoute, entityMetadata);
        }
    }
}
