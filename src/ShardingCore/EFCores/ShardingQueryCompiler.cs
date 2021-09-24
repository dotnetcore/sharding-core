using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;


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
            return _shardingQueryExecutor.Execute<TResult>(_currentContext, query);
        }


#if !EFCORE2

        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            return _shardingQueryExecutor.ExecuteAsync<TResult>(_currentContext, query, cancellationToken);
        }

        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        public Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        public Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

#endif

#if EFCORE2


        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            return _shardingQueryExecutor.ExecuteAsync<IAsyncEnumerable<TResult>>(_currentContext, query);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            return _shardingQueryExecutor.ExecuteAsync<Task<TResult>>(_currentContext, query, cancellationToken);
        }

        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        public Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        public Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
#endif

    }
}