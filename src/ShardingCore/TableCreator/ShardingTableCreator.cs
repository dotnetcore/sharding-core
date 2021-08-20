using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.TableCreator
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 21 December 2020 11:23:22
* @Email: 326308290@qq.com
*/
    public class ShardingTableCreator : IShardingTableCreator
    {
        private readonly ILogger<ShardingTableCreator> _logger;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IShardingConfigOption> _shardingConfigOptions;

        public ShardingTableCreator(ILogger<ShardingTableCreator> logger, IShardingDbContextFactory shardingDbContextFactory,
            IVirtualTableManager virtualTableManager, IServiceProvider serviceProvider, IEnumerable<IShardingConfigOption> shardingConfigOptions)
        {
            _logger = logger;
            _shardingDbContextFactory = shardingDbContextFactory;
            _virtualTableManager = virtualTableManager;
            _serviceProvider = serviceProvider;
            _shardingConfigOptions = shardingConfigOptions;
        }

        public void CreateTable<TShardingDbContext, T>(string tail) where TShardingDbContext : DbContext, IShardingDbContext where T : class, IShardingTable
        {
             CreateTable(typeof(TShardingDbContext),typeof(T), tail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shardingDbContextType"></param>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        /// <exception cref="ShardingCreateException"></exception>
        public void CreateTable(Type shardingDbContextType,Type shardingEntityType, string tail)
        {
            if (!shardingDbContextType.IsShardingDbContext())
                throw new ShardingCoreException(
                    $"{shardingDbContextType.FullName} must impl {nameof(IShardingDbContext)}");

            var shardingConfigOptions = _shardingConfigOptions.FirstOrDefault(o => o.ShardingDbContextType == shardingDbContextType);
            if (shardingConfigOptions == null)
                throw new ShardingCoreException(
                    "not found sharding config options db context is {shardingDbContextType.FullName}");
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var virtualTable = _virtualTableManager.GetVirtualTable(shardingDbContextType, shardingEntityType);
                var dbContext = (DbContext)serviceScope.ServiceProvider.GetService(shardingDbContextType);
                var shardingDbContext = (IShardingDbContext)dbContext;
                var context = shardingDbContext.GetDbContext(true,tail);

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
                            if (!shardingConfigOptions.IgnoreCreateTableError.GetValueOrDefault())
                            {
                                _logger.LogWarning(
                                    $"create table error maybe table:[{virtualTable.GetOriginalTableName()}{virtualTable.ShardingConfig.TailPrefix}{tail}]");
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