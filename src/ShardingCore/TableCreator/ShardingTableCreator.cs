using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Threading;
using ShardingCore.Logger;

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
        private static readonly ILogger<ShardingTableCreator> _logger =
            InternalLoggerFactory.CreateLogger<ShardingTableCreator>();

        private readonly IServiceProvider _serviceProvider;
        private readonly IShardingRouteConfigOptions _routeConfigOptions;
        private readonly IRouteTailFactory _routeTailFactory;

        public ShardingTableCreator(IServiceProvider serviceProvider,
            IShardingRouteConfigOptions routeConfigOptions, IRouteTailFactory routeTailFactory)
        {
            _serviceProvider = serviceProvider;
            _routeConfigOptions = routeConfigOptions;
            _routeTailFactory = routeTailFactory;
        }

        public void CreateTable<T>(IShardingDbContext shardingDbContext, string dataSourceName, string tail)
            where T : class
        {
            CreateTable(shardingDbContext, dataSourceName, typeof(T), tail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shardingDbContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        public void CreateTable(IShardingDbContext shardingDbContext, string dataSourceName, Type shardingEntityType,
            string tail)
        {
            using (var context = shardingDbContext.GetDbContext(dataSourceName, false,
                       _routeTailFactory.Create(tail, false)))
            {
                context.RemoveDbContextRelationModelSaveOnlyThatIsNamedType(shardingEntityType);
                var databaseCreator = context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                try
                {
                    databaseCreator.CreateTables();
                }
                catch (Exception ex)
                {
                    if (!_routeConfigOptions.IgnoreCreateTableError.GetValueOrDefault())
                    {
                        _logger.LogWarning(ex,
                            $"create table error entity name:[{shardingEntityType.Name}].");
                        throw new ShardingCoreException($" create table error :{ex.Message}", ex);
                    }
                }
            }
        }
    }
}