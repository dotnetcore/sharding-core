using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableCreator;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 21 December 2020 09:10:07
    * @Email: 326308290@qq.com
    */
    public class ShardingBootstrapper : IShardingBootstrapper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IShardingConfigOption> _shardingConfigOptions;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingTableCreator _tableCreator;
        private readonly ILogger<ShardingBootstrapper> _logger;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;

        public ShardingBootstrapper(IServiceProvider serviceProvider, IEnumerable<IShardingConfigOption> shardingConfigOptions,
            IVirtualTableManager virtualTableManager
            , IShardingTableCreator tableCreator, ILogger<ShardingBootstrapper> logger,
            IShardingDbContextFactory shardingDbContextFactory,IRouteTailFactory routeTailFactory)
        {
            ShardingContainer.SetServices(serviceProvider);
            _serviceProvider = serviceProvider;
            _shardingConfigOptions = shardingConfigOptions;
            _virtualTableManager = virtualTableManager;
            _tableCreator = tableCreator;
            _logger = logger;
            _routeTailFactory = routeTailFactory;
            _shardingDbContextFactory = shardingDbContextFactory;
        }

        public void Start()
        {
            

            using (var scope = _serviceProvider.CreateScope())
            {
                foreach (var shardingConfigOption in _shardingConfigOptions)
                {
                    using var context =
                        (DbContext) scope.ServiceProvider.GetService(shardingConfigOption.ShardingDbContextType);
                    EnsureCreated(context);
                    foreach (var entity in context.Model.GetEntityTypes())
                    {
                        if (entity.ClrType.IsShardingTable())
                        {
                            var routeType = shardingConfigOption.GetVirtualRouteType(entity.ClrType);
                            var virtualRoute = CreateVirtualRoute(routeType);
                            var virtualTable = CreateVirtualTable(entity.ClrType, virtualRoute);

                            //获取ShardingEntity的实际表名
                            var tableName = context.Model.FindEntityType(virtualTable.EntityType).GetTableName();
                            virtualTable.SetOriginalTableName(tableName);
                            _virtualTableManager.AddVirtualTable(shardingConfigOption.ShardingDbContextType,virtualTable);
                            CreateDataTable(shardingConfigOption.ShardingDbContextType,virtualTable, shardingConfigOption);
                        }
                    }

                }
            }
        }


        private IVirtualTableRoute CreateVirtualRoute(Type virtualRouteType)
        {
            var constructors
                = virtualRouteType.GetTypeInfo().DeclaredConstructors
                    .Where(c => !c.IsStatic && c.IsPublic)
                    .ToArray();
            if (constructors.IsEmpty())
            {
                object o = Activator.CreateInstance(virtualRouteType);
                return (IVirtualTableRoute) o;
            }
            else
            {
                if (constructors.Length > 1)
                {
                    throw new ArgumentException(
                        $"virtual route :[{virtualRouteType}] found more  declared constructor ");
                }

                var @params = constructors[0].GetParameters().Select(x => _serviceProvider.GetService(x.ParameterType))
                    .ToArray();
                object o = Activator.CreateInstance(virtualRouteType, @params);
                return (IVirtualTableRoute) o;
            }
        }

        private IVirtualTable CreateVirtualTable(Type entityType, IVirtualTableRoute virtualTableRoute)
        {
            Type type = typeof(OneDbVirtualTable<>);
            type = type.MakeGenericType(entityType);
            object o = Activator.CreateInstance(type, virtualTableRoute);
            return (IVirtualTable) o;
        }

        private  void EnsureCreated(DbContext context)
        {
            if (context is IShardingDbContext shardingDbContext)
            {
                var dbContext = shardingDbContext.GetDbContext(false,_routeTailFactory.Create(string.Empty));
                var modelCacheSyncObject = dbContext.GetModelCacheSyncObject();

                lock (modelCacheSyncObject)
                {
                    dbContext.RemoveDbContextRelationModelThatIsShardingTable();
                     dbContext.Database.EnsureCreated();
                    dbContext.RemoveModelCache();
                }
            }
        }

        private bool NeedCreateTable(ShardingTableConfig config, IShardingConfigOption shardingConfigOption)
        {
            if (config.AutoCreateTable.HasValue)
            {
                return config.AutoCreateTable.Value;
            }

            return shardingConfigOption.CreateShardingTableOnStart.GetValueOrDefault();
        }

        private void CreateDataTable(Type shardingDbContextType,IVirtualTable virtualTable,IShardingConfigOption shardingConfigOption)
        {
            var shardingConfig = virtualTable.ShardingConfig;
            foreach (var tail in virtualTable.GetVirtualRoute().GetAllTails())
            {
                if (NeedCreateTable(shardingConfig, shardingConfigOption))
                {
                    try
                    {
                        //添加物理表
                        virtualTable.AddPhysicTable(new DefaultPhysicTable(virtualTable, tail));
                        _tableCreator.CreateTable(shardingDbContextType,virtualTable.EntityType, tail);
                    }
                    catch (Exception e)
                    {
                        if (!shardingConfigOption.IgnoreCreateTableError.GetValueOrDefault())
                        {
                            _logger.LogWarning(
                                $"table :{virtualTable.GetOriginalTableName()}{shardingConfig.TailPrefix}{tail} will created.",e);
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