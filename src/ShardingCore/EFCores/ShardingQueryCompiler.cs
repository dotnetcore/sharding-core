using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;


namespace ShardingCore.EFCores
{
    /// <summary>
    /// 当前查询编译拦截
    /// </summary>
    public class ShardingQueryCompiler : QueryCompiler,IShardingDbContextAvailable
    {
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IShardingCompilerExecutor _shardingCompilerExecutor;
//
#if !EFCORE2
        public ShardingQueryCompiler(IShardingRuntimeContext shardingRuntimeContext,IQueryContextFactory queryContextFactory, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator, IDatabase database, IDiagnosticsLogger<DbLoggerCategory.Query> logger, ICurrentDbContext currentContext, IEvaluatableExpressionFilter evaluatableExpressionFilter, IModel model) 
            : base(queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger, currentContext, evaluatableExpressionFilter, model)
        {
            _shardingDbContext = currentContext.Context as IShardingDbContext ??
                                 throw new ShardingCoreException("db context operator is not IShardingDbContext");
            _shardingCompilerExecutor = shardingRuntimeContext.GetShardingCompilerExecutor();
        } 
#endif
#if EFCORE2
        

        public ShardingQueryCompiler(IShardingRuntimeContext shardingRuntimeContext,IQueryContextFactory queryContextFactory, ICompiledQueryCache compiledQueryCache, ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator, IDatabase database, IDiagnosticsLogger<DbLoggerCategory.Query> logger, ICurrentDbContext currentContext, IQueryModelGenerator queryModelGenerator) 
            : base(queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger, currentContext, queryModelGenerator)
        {
            _shardingDbContext = currentContext.Context as IShardingDbContext ??
                                 throw new ShardingCoreException("db context operator is not IShardingDbContext");
            _shardingCompilerExecutor = shardingRuntimeContext.GetShardingCompilerExecutor();
        }
#endif
        // public ShardingQueryCompiler(IShardingRuntimeContext shardingRuntimeContext,ICurrentDbContext currentContext)
        // {
        //      _shardingDbContext = currentContext.Context as IShardingDbContext ??
        //                           throw new ShardingCoreException("db context operator is not IShardingDbContext");
        //      _shardingCompilerExecutor = shardingRuntimeContext.GetShardingCompilerExecutor();
        //     
        // }

        public override TResult Execute<TResult>(Expression query)
        {
            return _shardingCompilerExecutor.Execute<TResult>(_shardingDbContext, query);
        }


#if !EFCORE2

        public override TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            return _shardingCompilerExecutor.ExecuteAsync<TResult>(_shardingDbContext, query, cancellationToken);
        }

        [ExcludeFromCodeCoverage]
        public override Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public override Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

#endif

#if EFCORE2
        public override IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            return _shardingCompilerExecutor.ExecuteAsync<TResult>(_shardingDbContext, query);
        }

        public override Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            return _shardingCompilerExecutor.ExecuteAsync<TResult>(_shardingDbContext, query, cancellationToken);
        }
        
        [ExcludeFromCodeCoverage]
        public override Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
        
        [ExcludeFromCodeCoverage]
        public override Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
        
        [ExcludeFromCodeCoverage]
        public override Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
#endif
        public IShardingDbContext GetShardingDbContext()
        {
            return _shardingDbContext;
        }
    }
}