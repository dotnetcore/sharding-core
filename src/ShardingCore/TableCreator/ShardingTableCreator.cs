using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Exceptions;

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

        public ShardingTableCreator(ILogger<ShardingTableCreator> logger, IShardingDbContextFactory shardingDbContextFactory, IVirtualTableManager virtualTableManager, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _shardingDbContextFactory = shardingDbContextFactory;
            _virtualTableManager = virtualTableManager;
            _serviceProvider = serviceProvider;
        }

        public void CreateTable<T>(string tail) where T : class, IShardingEntity
        {
             CreateTable(typeof(T), tail);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        /// <exception cref="ShardingCreateException"></exception>
        public void CreateTable(Type shardingEntityType, string tail)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var dbContextOptionsProvider = serviceScope.ServiceProvider.GetService<IDbContextOptionsProvider>();
                var virtualTable = _virtualTableManager.GetVirtualTable(shardingEntityType);

                using (var dbContext = _shardingDbContextFactory.Create(new ShardingDbContextOptions(dbContextOptionsProvider.GetDbContextOptions(), tail,
                    new List<VirtualTableDbContextConfig>() {new VirtualTableDbContextConfig(shardingEntityType, virtualTable.GetOriginalTableName(), virtualTable.ShardingConfig.TailPrefix)})))
                {
                    var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                    try
                    {
                        databaseCreator.CreateTables();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"create table error maybe table:[{virtualTable.GetOriginalTableName()}_{virtualTable.ShardingConfig.TailPrefix}_{tail}]");
                        throw new ShardingCreateException(" create table error :", ex);
                    }

                }
            }
        }
    }
}