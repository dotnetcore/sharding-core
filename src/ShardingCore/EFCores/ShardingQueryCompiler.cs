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


namespace ShardingCore.EFCores
{
    /// <summary>
    /// 当前查询编译拦截
    /// </summary>
    public class ShardingQueryCompiler : IQueryCompiler
    {
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IShardingCompilerExecutor _shardingCompilerExecutor;

        public ShardingQueryCompiler(ICurrentDbContext currentContext)
        {
            _shardingDbContext = currentContext.Context as IShardingDbContext ??
                                 throw new ShardingCoreException("db context operator is not IShardingDbContext");
            _shardingCompilerExecutor = ShardingContainer.GetService<IShardingCompilerExecutor>();
        }

        public TResult Execute<TResult>(Expression query)
        {
            return _shardingCompilerExecutor.Execute<TResult>(_shardingDbContext, query);
        }


#if !EFCORE2

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            return _shardingCompilerExecutor.ExecuteAsync<TResult>(_shardingDbContext, query, cancellationToken);
        }

        [ExcludeFromCodeCoverage]
        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
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
            return _shardingCompilerExecutor.ExecuteAsync<TResult>(_shardingDbContext, query);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            return _shardingCompilerExecutor.ExecuteAsync<TResult>(_shardingDbContext, query, cancellationToken);
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