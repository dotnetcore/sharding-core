using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.TableCreator
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 21 December 2020 11:23:22
    * @Email: 326308290@qq.com
    */
    public class ShardingTableCreator<TShardingDbContext> : IShardingTableCreator<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ILogger<ShardingTableCreator<TShardingDbContext>> _logger;
        private readonly IShardingDbContextFactory<TShardingDbContext> _shardingDbContextFactory;
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IShardingConfigOption _shardingConfigOption;
        private readonly IRouteTailFactory _routeTailFactory;

        public ShardingTableCreator(ILogger<ShardingTableCreator<TShardingDbContext>> logger, IShardingDbContextFactory<TShardingDbContext> shardingDbContextFactory,
            IVirtualTableManager<TShardingDbContext> virtualTableManager, IServiceProvider serviceProvider, IEnumerable<IShardingConfigOption> shardingConfigOptions,IRouteTailFactory routeTailFactory)
        {
            _logger = logger;
            _shardingDbContextFactory = shardingDbContextFactory;
            _virtualTableManager = virtualTableManager;
            _serviceProvider = serviceProvider;
            _shardingConfigOption = shardingConfigOptions.FirstOrDefault(o => o.ShardingDbContextType == typeof(TShardingDbContext))
                                     ??throw new ArgumentNullException(typeof(TShardingDbContext).FullName);
            _routeTailFactory = routeTailFactory;
        }

        public void CreateTable< T>(string dataSourceName, string tail) where T : class, IShardingTable
        {
             CreateTable(dataSourceName,typeof(T), tail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        /// <exception cref="ShardingCreateException"></exception>
        public void CreateTable(string dataSourceName,Type shardingEntityType, string tail)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var virtualTable = _virtualTableManager.GetVirtualTable( shardingEntityType);
                var dbContext = serviceScope.ServiceProvider.GetService<TShardingDbContext>();
                var shardingDbContext = (IShardingDbContext)dbContext;
                var context = shardingDbContext.GetDbContext(dataSourceName,false, _routeTailFactory.Create(tail));

                var modelCacheSyncObject = context.GetModelCacheSyncObject();
                    
                    lock (modelCacheSyncObject)
                    {
                        context.RemoveDbContextRelationModelSaveOnlyThatIsNamedType(shardingEntityType);
                        var databaseCreator = context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                        try
                        {
                            databaseCreator.CreateTables();
                        }
                        catch (Exception ex)
                        {
                            if (!_shardingConfigOption.IgnoreCreateTableError.GetValueOrDefault())
                            {
                                _logger.LogWarning(
                                    $"create table error maybe table:[{virtualTable.GetVirtualTableName()}{virtualTable.EntityMetadata.TableSeparator}{tail}]");
                                throw new ShardingCreateException(" create table error :", ex);
                            }
                        }
                        finally
                        {
                            context.RemoveModelCache();
                        }
                    }

            }
        }
    }
}