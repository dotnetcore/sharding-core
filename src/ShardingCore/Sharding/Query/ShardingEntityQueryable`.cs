using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.Query
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 14:17:30
    * @Email: 326308290@qq.com
    */
    public class ShardingEntityQueryable<TResult> : IOrderedQueryable<TResult>,
        IAsyncEnumerable<TResult>,
        IListSource
    {
        private readonly IAsyncQueryProvider _queryProvider;

        //private readonly DbContext _dbContext;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ShardingEntityQueryable(IAsyncQueryProvider queryProvider, IEntityType entityType)
            : this(queryProvider, new QueryRootExpression(queryProvider, entityType))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ShardingEntityQueryable(IAsyncQueryProvider queryProvider, Expression expression)
        {
            Check.NotNull(queryProvider, nameof(queryProvider));
            Check.NotNull(expression, nameof(expression));
            //if (queryProvider is ShardingEntityQueryProvider shardingEntityQueryProvider)
            //{
            //    _dbContext = shardingEntityQueryProvider.GetCurrentDbContext().Context;
            //}

            _queryProvider = queryProvider;
            Expression = expression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();

        }
        public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return _queryProvider.ExecuteAsync<IAsyncEnumerable<TResult>>(Expression).GetAsyncEnumerator(cancellationToken);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType => typeof(TResult);
        public Expression Expression { get; }
        public IQueryProvider Provider => _queryProvider;

        public IList GetList()
        {
            throw new NotSupportedException(CoreStrings.DataBindingWithIListSource);
        }

        public bool ContainsListCollection => false;
    }
}