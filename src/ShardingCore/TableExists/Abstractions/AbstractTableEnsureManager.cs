using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Data.Common;
using ShardingCore.Extensions;
using ShardingCore.Sharding;

namespace ShardingCore.TableExists.Abstractions
{
    public abstract class AbstractTableEnsureManager : ITableEnsureManager
    {
        protected IRouteTailFactory RouteTailFactory { get; }
        protected AbstractTableEnsureManager(IRouteTailFactory routeTailFactory)
        {
            RouteTailFactory = routeTailFactory;
        }

        public ISet<string> GetExistTables(IShardingDbContext shardingDbContext, string dataSourceName)
        {
            using (var dbContext =
                   shardingDbContext.GetIndependentWriteDbContext(dataSourceName, RouteTailFactory.Create(string.Empty)))
            {
                var dbConnection = dbContext.Database.GetDbConnection();
                dbConnection.Open();
                return DoGetExistTables(dbConnection, dataSourceName);
            }
        }

        public abstract ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName);
    }
}
