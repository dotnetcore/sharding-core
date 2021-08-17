//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Threading;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using Microsoft.EntityFrameworkCore.Query;
//using Microsoft.EntityFrameworkCore.Query.Internal;
//using ShardingCore.Exceptions;
//using ShardingCore.Extensions;
//using ShardingCore.Sharding.Abstractions;
//using ShardingCore.Sharding.Enumerators;

//namespace ShardingCore.Sharding.Query
//{
///*
//* @Author: xjm
//* @Description:
//* @Date: Saturday, 14 August 2021 14:25:27
//* @Email: 326308290@qq.com
//*/
//    public class ShardingEntityQueryProvider: IAsyncQueryProvider
//    {
//        private static readonly MethodInfo _genericCreateQueryMethod
//            = typeof(ShardingEntityQueryProvider).GetRuntimeMethods()
//                .Single(m => (m.Name == "CreateQuery") && m.IsGenericMethod);

//        private readonly MethodInfo _genericExecuteMethod;

//        private readonly IQueryCompiler _queryCompiler;
//        private readonly ICurrentDbContext _currentDbContext;
//        private readonly IStreamMergeContextFactory _streamMergeContextFactory;

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public ShardingEntityQueryProvider(IQueryCompiler queryCompiler,ICurrentDbContext currentDbContext)
//        {
//            _queryCompiler = queryCompiler;
//            _currentDbContext = currentDbContext;
//            _genericExecuteMethod = queryCompiler.GetType()
//                .GetRuntimeMethods()
//                .Single(m => (m.Name == "Execute") && m.IsGenericMethod);
//            _streamMergeContextFactory = ShardingContainer.GetService<IStreamMergeContextFactory>();
//        }

//        public ICurrentDbContext GetCurrentDbContext()
//        {
//            return _currentDbContext;
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
//            => new ShardingEntityQueryable<TElement>(this, expression);

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual IQueryable CreateQuery(Expression expression)
//            => (IQueryable)_genericCreateQueryMethod
//                .MakeGenericMethod(expression.Type.GetSequenceType())
//                .Invoke(this, new object[] { expression });

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual TResult Execute<TResult>(Expression expression)
//            => throw new NotSupportedException();

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual object Execute(Expression expression)
//            => throw new NotSupportedException();

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
//        {
//            var currentDbContext = GetCurrentDbContext();

//            if (currentDbContext is IShardingDbContext shardingDbContext)
//            {
//                if (typeof(TResult).HasImplementedRawGeneric(typeof(IAsyncEnumerable<>)))
//                {
//                    var constructors
//                        = typeof(EnumerableQuery<>).GetTypeInfo().DeclaredConstructors
//                            .Where(c => !c.IsStatic && c.IsPublic)
//                            .ToArray();

//                    var parameters = constructors[0].GetParameters();
//                    var parameterType = parameters[0].ParameterType;
//                    Type type = typeof(EnumerableQuery<>);
//                    type = type.MakeGenericType(ActualDbContextType);
//                    Activator.CreateInstance(type);
//                }

//                IQueryable<TResult> queryable = new EnumerableQuery<TResult>(expression);
//                var streamMergeContext = _streamMergeContextFactory.Create(queryable, shardingDbContext);

//                var streamMergeEngine = AsyncEnumerableStreamMergeEngine<TResult>.Create<TResult>(streamMergeContext);
//                return streamMergeEngine.GetAsyncEnumerator();
//            }

//            throw new ShardingCoreException("db context is not IShardingDbContext");
//        }
//    }
//}
