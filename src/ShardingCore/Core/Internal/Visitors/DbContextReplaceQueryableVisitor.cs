using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 16:32:27
* @Email: 326308290@qq.com
*/
#if !EFCORE5
    internal class DbContextReplaceQueryableVisitor : ExpressionVisitor
    {
        private readonly DbContext _dbContext;
        public IQueryable Source;

        public DbContextReplaceQueryableVisitor(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable)
            {
                var dbContextDependencies = typeof(DbContext).GetTypePropertyValue(_dbContext, "DbContextDependencies") as IDbContextDependencies;
                var targetIQ = (IQueryable)((IDbSetCache)_dbContext).GetOrAddSet(dbContextDependencies.SetSource, queryable.ElementType);
                var newQueryable = targetIQ.Provider.CreateQuery((Expression) Expression.Call((Expression) null, typeof(EntityFrameworkQueryableExtensions).GetTypeInfo().GetDeclaredMethod("AsNoTracking").MakeGenericMethod(queryable.ElementType), targetIQ.Expression));
                Source = newQueryable;
return base.Visit(Expression.Constant(newQueryable));
                 // return Expression.Constant(newQueryable);
            }

            return base.VisitConstant(node);
        }
    }
#endif

#if EFCORE5

    internal class DbContextReplaceQueryableVisitor : ExpressionVisitor
    {
        private readonly DbContext _dbContext;
        public IQueryable Source;

        public DbContextReplaceQueryableVisitor(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is QueryRootExpression queryRootExpression)
            {
                var dbContextDependencies = typeof(DbContext).GetTypePropertyValue(_dbContext, "DbContextDependencies") as IDbContextDependencies;
                var targetIQ = (IQueryable) ((IDbSetCache) _dbContext).GetOrAddSet(dbContextDependencies.SetSource, queryRootExpression.EntityType.ClrType);
                var newQueryable = targetIQ.Provider.CreateQuery((Expression) Expression.Call((Expression) null, typeof(EntityFrameworkQueryableExtensions).GetTypeInfo().GetDeclaredMethod("AsNoTracking").MakeGenericMethod(queryRootExpression.EntityType.ClrType), targetIQ.Expression));
                Source = newQueryable;
                //如何替换ef5的set
                var replaceQueryRoot = new ReplaceSingleQueryRootExpressionVisitor();
                replaceQueryRoot.Visit(newQueryable.Expression);
                return base.VisitExtension(replaceQueryRoot.QueryRootExpression);
            }

            return base.VisitExtension(node);
        }

        class ReplaceSingleQueryRootExpressionVisitor : ExpressionVisitor
        {
            public QueryRootExpression QueryRootExpression { get; set; }

            protected override Expression VisitExtension(Expression node)
            {
                if (node is QueryRootExpression queryRootExpression)
                {
                    if (QueryRootExpression != null)
                        throw new InvalidReplaceQueryRootException("more than one query root");
                    QueryRootExpression = queryRootExpression;
                }

                return base.VisitExtension(node);
            }
        }
    }
#endif
    // class ReplaceQueryableVisitor : ExpressionVisitor
    // {
    //     private readonly QueryRootExpression _queryRootExpression;
    //     public ReplaceQueryableVisitor(IQueryable newQuery)
    //     {
    //         var visitor = new GetQueryRootVisitor();
    //         visitor.Visit(newQuery.Expression);
    //         _queryRootExpression = visitor.QueryRootExpression;
    //     }
    //
    //     protected override Expression VisitExtension(Expression node)
    //     {
    //         if (node is QueryRootExpression)
    //         {
    //             return _queryRootExpression;
    //         }
    //
    //         return base.VisitExtension(node);
    //     }
    // }
    // class GetQueryRootVisitor : ExpressionVisitor
    // {
    //     public QueryRootExpression QueryRootExpression { get; set; }
    //     protected override Expression VisitExtension(Expression node)
    //     {
    //         if (node is QueryRootExpression expression)
    //         {
    //             QueryRootExpression = expression;
    //         }
    //
    //         return base.VisitExtension(node);
    //     }
    // }
}