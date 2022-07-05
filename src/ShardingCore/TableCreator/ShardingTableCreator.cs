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
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Logger;
using ShardingCore.Sharding;

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
            ShardingLoggerFactory.CreateLogger<ShardingTableCreator>();

        private readonly IShardingProvider _shardingProvider;
        private readonly IShardingRouteConfigOptions _routeConfigOptions;
        private readonly IRouteTailFactory _routeTailFactory;
        private readonly IDbContextCreator _dbContextCreator;

        public ShardingTableCreator(IShardingProvider shardingProvider,IShardingRouteConfigOptions routeConfigOptions, IRouteTailFactory routeTailFactory,IDbContextCreator dbContextCreator)
        {
            _shardingProvider = shardingProvider;
            _routeConfigOptions = routeConfigOptions;
            _routeTailFactory = routeTailFactory;
            _dbContextCreator = dbContextCreator;
        }

        public void CreateTable<T>(string dataSourceName, string tail)
            where T : class
        {
            CreateTable(dataSourceName, typeof(T), tail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        public void CreateTable(string dataSourceName, Type shardingEntityType,
            string tail)
        {
            using (var scope = _shardingProvider.CreateScope())
            {
                
                using (var shellDbContext = _dbContextCreator.GetShellDbContext(scope.ServiceProvider))
                {
                    using (var context = ((IShardingDbContext)shellDbContext).GetIndependentWriteDbContext(dataSourceName,
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
    }
}