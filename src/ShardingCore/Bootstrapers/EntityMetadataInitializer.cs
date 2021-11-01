using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.EntityShardingMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Jobs;
using ShardingCore.Jobs.Abstaractions;
using ShardingCore.Jobs.Impls;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableCreator;
using ShardingCore.Utils;

namespace ShardingCore.Bootstrapers
{
    public interface IEntityMetadataInitializer
    {
        void Initialize();
    }
    public class EntityMetadataInitializer<TShardingDbContext,TEntity>: IEntityMetadataInitializer where TShardingDbContext:DbContext,IShardingDbContext where TEntity:class
    {
        private readonly IEntityType _entityType;
        private readonly string _virtualTableName;
        private readonly IShardingConfigOption _shardingConfigOption;
        private readonly ITrackerManager<TShardingDbContext> _trackerManager;
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private readonly IShardingTableCreator<TShardingDbContext> _tableCreator;
        private readonly ILogger<EntityMetadataInitializer<TShardingDbContext, TEntity>> _logger;

        public EntityMetadataInitializer(EntityMetadataEnsureParams entityMetadataEnsureParams
            , IShardingConfigOption shardingConfigOption,
            ITrackerManager<TShardingDbContext> trackerManager,IVirtualDataSource<TShardingDbContext> virtualDataSource,IVirtualTableManager<TShardingDbContext> virtualTableManager,
            IEntityMetadataManager<TShardingDbContext> entityMetadataManager, IShardingTableCreator<TShardingDbContext> tableCreator,
            ILogger<EntityMetadataInitializer<TShardingDbContext, TEntity>> logger
            )
        {
            _entityType = entityMetadataEnsureParams.EntityType;
            _virtualTableName = entityMetadataEnsureParams.VirtualTableName;
            _shardingConfigOption = shardingConfigOption;
            _trackerManager = trackerManager;
            _virtualDataSource = virtualDataSource;
            _virtualTableManager = virtualTableManager;
            _entityMetadataManager = entityMetadataManager;
            _tableCreator = tableCreator;
            _logger = logger;
        }
        public void Initialize()
        {
            var shardingEntityType = _entityType.ClrType;
            _trackerManager.AddDbContextModel(shardingEntityType);
            var entityMetadata = new EntityMetadata(shardingEntityType, _virtualTableName,typeof(TShardingDbContext),_entityType.FindPrimaryKey().Properties.Select(o=>o.PropertyInfo).ToList());
            if (!_entityMetadataManager.AddEntityMetadata(entityMetadata))
                throw new InvalidOperationException($"repeat add entity metadata {shardingEntityType.FullName}");
            //设置标签
            if (_shardingConfigOption.TryGetVirtualDataSourceRoute<TEntity>(out var virtualDataSourceRouteType))
            {
                var creatEntityMetadataDataSourceBuilder = EntityMetadataDataSourceBuilder<TEntity>.CreatEntityMetadataDataSourceBuilder(entityMetadata);
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

                _virtualDataSource.AddVirtualDataSourceRoute(dataSourceRoute);

            }
            if (_shardingConfigOption.TryGetVirtualTableRoute<TEntity>(out var virtualTableRouteType))
            {
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
                entityMetadata.CheckMetadata();
                //添加任务
                if (virtualTableRoute is IJob routeJob && routeJob.StartJob())
                {
                    var jobManager = ShardingContainer.GetService<IJobManager>();
                    var jobEntries = JobTypeParser.Parse(virtualTableRoute.GetType());
                    jobEntries.ForEach(o =>
                    {
                        o.JobName = $"{routeJob.JobName}:{o.JobName}";
                    });
                    foreach (var jobEntry in jobEntries)
                    {
                        jobManager.AddJob(jobEntry);
                    }
                }
            }
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

            var @params = constructors[0].GetParameters().Select(x => x.ParameterType == ShardingContainer.GetService(x.ParameterType))
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
