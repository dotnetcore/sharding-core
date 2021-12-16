using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Utils;


namespace ShardingCore.EFCores
{
    /**
	 * 描述：
	 * 
	 * Author：xuejiaming
	 * Created: 2020/12/28 13:58:46
	 **/
    public class ShardingQueryCompiler : IQueryCompiler
    {
        private readonly ICurrentDbContext _currentContext;
        private readonly IShardingQueryExecutor _shardingQueryExecutor;

        public ShardingQueryCompiler(ICurrentDbContext currentContext)
        {
            _currentContext = currentContext;
            _shardingQueryExecutor = ShardingContainer.GetService<IShardingQueryExecutor>();
        }

        public TResult Execute<TResult>(Expression query)
        {
            if (_currentContext.Context is IShardingDbContext shardingDbContext)
            {
                var queryCompilerIfNoShardingQuery = GetQueryCompilerIfNoShardingQuery(shardingDbContext, query);
                if (queryCompilerIfNoShardingQuery != null)
                {
                    return queryCompilerIfNoShardingQuery.Execute<TResult>(query);
                }
                return _shardingQueryExecutor.Execute<TResult>(_currentContext, query);
            }
            throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }

        private IQueryCompiler GetQueryCompilerIfNoShardingQuery(IShardingDbContext shardingDbContext, Expression query)
        {
            var queryEntities = ShardingUtil.GetQueryEntitiesByExpression(query);
            var entityMetadataManager = (IEntityMetadataManager)ShardingContainer.GetService(typeof(IEntityMetadataManager<>).GetGenericType0(shardingDbContext.GetType()));
            if (queryEntities.All(o => !entityMetadataManager.IsSharding(o)))
            {
                var virtualDataSource = (IVirtualDataSource)ShardingContainer.GetService(
                    typeof(IVirtualDataSource<>).GetGenericType0(shardingDbContext.GetType()));
                var routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
                var dbContext = shardingDbContext.GetDbContext(virtualDataSource.DefaultDataSourceName, false, routeTailFactory.Create(string.Empty));
                return dbContext.GetService<IQueryCompiler>();
            }
            return null;
        }


#if !EFCORE2

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            if (_currentContext.Context is IShardingDbContext shardingDbContext)
            {
                var queryCompilerIfNoShardingQuery = GetQueryCompilerIfNoShardingQuery(shardingDbContext, query);
                if (queryCompilerIfNoShardingQuery != null)
                {
                    return queryCompilerIfNoShardingQuery.ExecuteAsync<TResult>(query, cancellationToken);
                }
                return _shardingQueryExecutor.ExecuteAsync<TResult>(_currentContext, query, cancellationToken);
            }
                throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }

        [ExcludeFromCodeCoverage]
        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

#endif

#if EFCORE2


        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            if (_currentContext.Context is IShardingDbContext shardingDbContext)
            {
                var queryCompilerIfNoShardingQuery = GetQueryCompilerIfNoShardingQuery(shardingDbContext, query);
                if (queryCompilerIfNoShardingQuery != null)
                {
                    return queryCompilerIfNoShardingQuery.ExecuteAsync<TResult>(query);
                }
                return _shardingQueryExecutor.ExecuteAsync<IAsyncEnumerable<TResult>>(_currentContext, query);
            }
            throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            if (_currentContext.Context is IShardingDbContext shardingDbContext)
            {
                var queryCompilerIfNoShardingQuery = GetQueryCompilerIfNoShardingQuery(shardingDbContext, query);
                if (queryCompilerIfNoShardingQuery != null)
                {
                    return queryCompilerIfNoShardingQuery.ExecuteAsync<TResult>(query,cancellationToken);
                }
                return _shardingQueryExecutor.ExecuteAsync<Task<TResult>>(_currentContext, query, cancellationToken);
            }
            throw new ShardingCoreException("db context operator is not IShardingDbContext");
        }
        
        [ExcludeFromCodeCoverage]
        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
        
        [ExcludeFromCodeCoverage]
        public Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
        
        [ExcludeFromCodeCoverage]
        public Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
#endif

    }
}