using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Utils;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class QueryCompilerContext
    {
        public ISet<Type> QueryEntities { get; }
        public IShardingDbContext ShardingDbContext { get; }
        public Expression QueryExpression { get; }
        public IEntityMetadataManager EntityMetadataManager { get; }
        public Type ShardingDbContextType { get; }
        private DbContext realDbContext;

        private QueryCompilerContext( IShardingDbContext shardingDbContext, Expression queryExpression)
        {
            ShardingDbContextType = shardingDbContext.GetType();
            QueryEntities = ShardingUtil.GetQueryEntitiesByExpression(queryExpression, ShardingDbContextType);
            ShardingDbContext = shardingDbContext;
            QueryExpression = queryExpression;
            EntityMetadataManager = (IEntityMetadataManager)ShardingContainer.GetService(typeof(IEntityMetadataManager<>).GetGenericType0(ShardingDbContextType));
        }

        public static QueryCompilerContext Create(IShardingDbContext shardingDbContext, Expression queryExpression)
        {
            return new QueryCompilerContext(shardingDbContext, queryExpression);
        }

        public IQueryCompiler GetQueryCompiler()
        {
            if (QueryEntities.All(o => !EntityMetadataManager.IsSharding(o)))
            {
                var virtualDataSource = (IVirtualDataSource)ShardingContainer.GetService(
                    typeof(IVirtualDataSource<>).GetGenericType0(ShardingDbContextType));
                var routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
                realDbContext = ShardingDbContext.GetDbContext(virtualDataSource.DefaultDataSourceName, false, routeTailFactory.Create(string.Empty));
                return realDbContext.GetService<IQueryCompiler>();
            }

            return null;
        }

        public Expression NewQueryExpression()
        {
            return QueryExpression.ReplaceDbContextExpression(realDbContext);
        }
    }
}
