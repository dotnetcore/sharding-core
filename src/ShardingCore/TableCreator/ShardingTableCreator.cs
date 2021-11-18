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
using System.Threading;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IShardingConfigOption _shardingConfigOption;
        private readonly IRouteTailFactory _routeTailFactory;

        public ShardingTableCreator(ILogger<ShardingTableCreator<TShardingDbContext>> logger,  IServiceProvider serviceProvider, IEnumerable<IShardingConfigOption> shardingConfigOptions, IRouteTailFactory routeTailFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _shardingConfigOption = shardingConfigOptions.FirstOrDefault(o => o.ShardingDbContextType == typeof(TShardingDbContext))
                                     ?? throw new ArgumentNullException(typeof(TShardingDbContext).FullName);
            _routeTailFactory = routeTailFactory;
        }

        public void CreateTable<T>(string dataSourceName, string tail) where T : class
        {
            CreateTable(dataSourceName, typeof(T), tail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        /// <exception cref="ShardingCreateException"></exception>
        public void CreateTable(string dataSourceName, Type shardingEntityType, string tail)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<TShardingDbContext>();
                var shardingDbContext = (IShardingDbContext)dbContext;
                var context = shardingDbContext.GetDbContext(dataSourceName, false, _routeTailFactory.Create(tail));

                var modelCacheSyncObject = context.GetModelCacheSyncObject();

                var acquire = Monitor.TryEnter(modelCacheSyncObject,TimeSpan.FromSeconds(3));
                if (!acquire)
                {
                    throw new ShardingCoreException("cant get modelCacheSyncObject lock");
                }

                try
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
                            _logger.LogWarning(ex,
                                $"create table error entity name:[{shardingEntityType.Name}].");
                            throw new ShardingCreateException($" create table error :{ex.Message}", ex);
                        }
                    }
                    finally
                    {
                        context.RemoveModelCache();
                    }
                }
                finally
                {
                    Monitor.Exit(modelCacheSyncObject);
                }

            }
        }
    }
}