using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;
using ShardingCore.Sharding.StreamMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Internal;
#endif

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

        private IAsyncEnumerable<TResult> AsyncEnumerableExecute<TResult>(IShardingDbContext shardingDbContext, Expression query)
        {
            Type queryEntityType = query.Type.GetSequenceType();
            Type type = typeof(EnumerableQuery<>);
            type = type.MakeGenericType(queryEntityType);
            var queryable = Activator.CreateInstance(type, query);

            var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
            if (streamMergeContextMethod == null)
                throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
            var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new[] { queryable, shardingDbContext });


            Type streamMergeEngineType = typeof(AsyncEnumerableStreamMergeEngine<>);
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
            return (IAsyncEnumerable<TResult>)Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
        }
        private Task<TResult> EnumerableExecuteAsync<TResult>(IShardingDbContext shardingDbContext, Expression query, bool async)
        {
            Type queryEntityType;
            if (async)
                queryEntityType = typeof(TResult).GetGenericArguments()[0];
            else
            {
                queryEntityType = query.Type.GetSequenceType();
            }
            Type type = typeof(EnumerableQuery<>);
            type = type.MakeGenericType(queryEntityType);
            var queryable = Activator.CreateInstance(type, query);

            var streamMergeContextMethod = _streamMergeContextFactory.GetType().GetMethod("Create");
            if (streamMergeContextMethod == null)
                throw new ShardingCoreException("cant found IStreamMergeContextFactory method [Create]");
            var streamMergeContext = streamMergeContextMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(_streamMergeContextFactory, new[] { queryable, shardingDbContext });


            Type streamMergeEngineType = typeof(AsyncEnumerableStreamMergeEngine<>);
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
            return (Task<TResult>)Activator.CreateInstance(streamMergeEngineType, streamMergeContext);
        }
        private Task<TResult> GenericMergeExecuteAsync<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            var queryEntityType = query.GetQueryEntityType();
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod(async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (Task<TResult>)streamEngineMethod.MakeGenericMethod(new Type[] { queryEntityType }).Invoke(streamEngine, @params);
        }


        private Task<TResult> EnsureMergeExecuteAsync<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType());
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod(async ? nameof(IEnsureMergeResult<object>.MergeResultAsync) : nameof(IEnsureMergeResult<object>.MergeResult));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (Task<TResult>)streamEngineMethod.Invoke(streamEngine, @params);
        }
        private Task<TResult> GenericMergeExecuteAsync2<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query, bool async, CancellationToken cancellationToken)
        {
            var queryEntityType = query.GetQueryEntityType();
            var resultType = query.GetResultType();
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(queryEntityType,resultType);
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod(async ? nameof(IGenericMergeResult.MergeResultAsync) : nameof(IGenericMergeResult.MergeResult));
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            var @params = async ? new object[] { cancellationToken } : new object[0];
            return (Task<TResult>)streamEngineMethod.MakeGenericMethod(new Type[] { resultType }).Invoke(streamEngine, @params);
        }


        private Task<TResult> EnsureMergeExecuteAsync2<TResult>(Type streamMergeEngineType, IShardingDbContext shardingDbContext, MethodCallExpression query,  CancellationToken cancellationToken)
        {
            streamMergeEngineType = streamMergeEngineType.MakeGenericType(query.GetQueryEntityType(), typeof(TResult));
            var streamEngine = Activator.CreateInstance(streamMergeEngineType, query, shardingDbContext);
            var streamEngineMethod = streamMergeEngineType.GetMethod(nameof(IEnsureMergeResult<object>.MergeResultAsync) );
            if (streamEngineMethod == null)
                throw new ShardingCoreException("cant found InMemoryAsyncStreamMergeEngine method [DoExecuteAsync]");
            var @params = new object[] { cancellationToken };
            return (Task<TResult>)streamEngineMethod.Invoke(streamEngine, @params);
        }

#endif

#if EFCORE2


        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            return _shardingQueryExecutor.ExecuteAsync<IAsyncEnumerable<TResult>>(_currentContext, query, cancellationToken);
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