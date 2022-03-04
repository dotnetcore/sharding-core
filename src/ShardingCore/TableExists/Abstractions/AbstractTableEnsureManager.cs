using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Data.Common;

namespace ShardingCore.TableExists.Abstractions
{
    public abstract class AbstractTableEnsureManager<TShardingDbContext> : ITableEnsureManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        protected IRouteTailFactory RouteTailFactory { get; }
        protected AbstractTableEnsureManager()
        {
            RouteTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
        }
        public ISet<string> GetExistTables(string dataSourceName)
        {
            using (var scope = ShardingContainer.ServiceProvider.CreateScope())
            {
                var shardingDbContext = scope.ServiceProvider.GetService<TShardingDbContext>();
                return GetExistTables(shardingDbContext, dataSourceName);
            }
        }

        public ISet<string> GetExistTables(IShardingDbContext shardingDbContext, string dataSourceName)
        {
            using (var dbContext =
                   shardingDbContext.GetDbContext(dataSourceName, true, RouteTailFactory.Create(string.Empty)))
            {
                var dbConnection = dbContext.Database.GetDbConnection();
                dbConnection.Open();
                return DoGetExistTables(dbConnection, dataSourceName);
            }
        }

        public abstract ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName);
    }
}
