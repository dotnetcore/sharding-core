using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableCreator;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/20 14:04:55
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingDbContextBootstrapper
    {
        void Initialize();
    }

    public class ShardingDbContextBootstrapper<TShardingDbContext>: IShardingDbContextBootstrapper where TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly IShardingConfigOption _shardingConfigOption;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IShardingTableCreator<TShardingDbContext> _tableCreator;
        private readonly ILogger<ShardingDbContextBootstrapper<TShardingDbContext>> _logger;

        public ShardingDbContextBootstrapper(IShardingConfigOption shardingConfigOption)
        {
            _shardingConfigOption = shardingConfigOption;
            _routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager<TShardingDbContext>>();
            _tableCreator=ShardingContainer.GetService<IShardingTableCreator<TShardingDbContext>>();
            _logger = ShardingContainer.GetService<ILogger<ShardingDbContextBootstrapper<TShardingDbContext>>>();
        }
        public void Initialize()
        {
            var virtualDataSource = ShardingContainer.GetService<IVirtualDataSource<TShardingDbContext>>();
            using (var serviceScope = ShardingContainer.Services.CreateScope())
            {
                var dataSources = _shardingConfigOption.GetDataSources();
                virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(_shardingConfigOption.DefaultDataSourceName, _shardingConfigOption.DefaultConnectionString,true));
                //foreach (var dataSourceKv in dataSources)
                //{
                //    virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceKv.Key,
                //        dataSourceKv.Value, false));
                //}
                foreach (var dataSourceKv in dataSources)
                {
                    var dataSourceName = dataSourceKv.Key;
                    var connectionString = dataSourceKv.Value;
                    virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName,
                        connectionString, false));
                    using var context =
                        (DbContext)serviceScope.ServiceProvider.GetService(_shardingConfigOption.ShardingDbContextType);
                    EnsureCreated(context, dataSourceName);
                    foreach (var entity in context.Model.GetEntityTypes())
                    {
                        var entityType = entity.ClrType;
                        if (entityType.IsShardingDataSource())
                        {
                            var routeType = _shardingConfigOption.GetVirtualDataSourceRouteType(entityType);
                            var virtualRoute = CreateVirtualDataSourceRoute(routeType);
                            virtualDataSource.AddVirtualDataSourceRoute(virtualRoute);
                        }
                        if (entityType.IsShardingTable())
                        {
                            var routeType = _shardingConfigOption.GetVirtualTableRouteType(entityType);
                            var virtualRoute = CreateVirtualTableRoute(routeType);
                            var virtualTable = CreateVirtualTable(entityType, virtualRoute);

                            //获取ShardingEntity的实际表名
                            var tableName = context.Model.FindEntityType(virtualTable.EntityType).GetTableName();
                            virtualTable.SetOriginalTableName(tableName);
                            _virtualTableManager.AddVirtualTable(virtualTable);
                            CreateDataTable(dataSourceName,virtualTable);
                        }
                    }
                }
            }
        }
        private void EnsureCreated(DbContext context,string dataSourceName)
        {
            if (context is IShardingDbContext shardingDbContext)
            {
                var dbContext = shardingDbContext.GetDbContext(dataSourceName,false, _routeTailFactory.Create(string.Empty));
                var modelCacheSyncObject = dbContext.GetModelCacheSyncObject();

                lock (modelCacheSyncObject)
                {
                    dbContext.RemoveDbContextRelationModelThatIsShardingTable();
                    dbContext.Database.EnsureCreated();
                    dbContext.RemoveModelCache();
                }
            }
        }



        private IVirtualDataSourceRoute CreateVirtualDataSourceRoute(Type virtualRouteType)
        {
            var constructors
                = virtualRouteType.GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();
            if (constructors.IsEmpty())
            {
                object o = Activator.CreateInstance(virtualRouteType);
                return (IVirtualDataSourceRoute)o;
            }
            else
            {
                if (constructors.Length > 1)
                {
                    throw new ArgumentException(
                        $"virtual route :[{virtualRouteType}] found more  declared constructor ");
                }

                var @params = constructors[0].GetParameters().Select(x => ShardingContainer.GetService(x.ParameterType))
                    .ToArray();
                object o = Activator.CreateInstance(virtualRouteType, @params);
                return (IVirtualDataSourceRoute)o;
            }
        }



        private IVirtualTableRoute CreateVirtualTableRoute(Type virtualRouteType)
        {
            var constructors
                = virtualRouteType.GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();
            if (constructors.IsEmpty())
            {
                object o = Activator.CreateInstance(virtualRouteType);
                return (IVirtualTableRoute)o;
            }
            else
            {
                if (constructors.Length > 1)
                {
                    throw new ArgumentException(
                        $"virtual route :[{virtualRouteType}] found more  declared constructor ");
                }

                var @params = constructors[0].GetParameters().Select(x => ShardingContainer.GetService(x.ParameterType))
                    .ToArray();
                object o = Activator.CreateInstance(virtualRouteType, @params);
                return (IVirtualTableRoute)o;
            }
        }

        private IVirtualTable CreateVirtualTable(Type entityType, IVirtualTableRoute virtualTableRoute)
        {
            Type type = typeof(DefaultVirtualTable<>);
            type = type.MakeGenericType(entityType);
            object o = Activator.CreateInstance(type, virtualTableRoute);
            return (IVirtualTable)o;
        }

        private bool NeedCreateTable(ShardingTableConfig config)
        {
            if (config.AutoCreateTable.HasValue)
            {
                return config.AutoCreateTable.Value;
            }

            return _shardingConfigOption.CreateShardingTableOnStart.GetValueOrDefault();
        }
        private void CreateDataTable(string dataSourceName,IVirtualTable virtualTable)
        {
            var shardingConfig = virtualTable.ShardingConfig;
            foreach (var tail in virtualTable.GetVirtualRoute().GetAllTails())
            {
                if (NeedCreateTable(shardingConfig))
                {
                    try
                    {
                        //添加物理表
                        virtualTable.AddPhysicTable(new DefaultPhysicTable(virtualTable, tail));
                        _tableCreator.CreateTable(dataSourceName,virtualTable.EntityType, tail);
                    }
                    catch (Exception e)
                    {
                        if (!_shardingConfigOption.IgnoreCreateTableError.GetValueOrDefault())
                        {
                            _logger.LogWarning(
                                $"table :{virtualTable.GetVirtualTableName()}{shardingConfig.TailPrefix}{tail} will created.", e);
                        }
                    }
                }
                else
                {
                    //添加物理表
                    virtualTable.AddPhysicTable(new DefaultPhysicTable(virtualTable, tail));
                }

            }
        }

    }
}
