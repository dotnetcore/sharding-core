using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core.Internal.Visitors;
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

    internal class DbContextInnerMemberReferenceReplaceQueryableVisitor : ExpressionVisitor
    {
        private readonly DbContext _dbContext;
        protected bool RootIsVisit = false;

        public DbContextInnerMemberReferenceReplaceQueryableVisitor(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Expression VisitMember
            (MemberExpression memberExpression)
        {
            // Recurse down to see if we can simplify...
            //if (memberExpression.IsMemberQueryable()) //2x,3x 路由 单元测试 分表和不分表
            //{
            var expression = Visit(memberExpression.Expression);

            // If we've ended up with a constant, and it's a property or a field,
            // we can simplify ourselves to a constant
            if (expression is ConstantExpression constantExpression)
            {
                object container = constantExpression.Value;
                var member = memberExpression.Member;
                if (member is FieldInfo fieldInfo)
                {
                    object value = fieldInfo.GetValue(container);
                    if (value is IQueryable queryable)
                    {
                        return ReplaceMemberExpression(queryable);
                    }

                    if (value is DbContext dbContext)
                    {
                        return ReplaceMemberExpression(dbContext);
                    }
                    //return Expression.Constant(value);
                }

                if (member is PropertyInfo propertyInfo)
                {
                    object value = propertyInfo.GetValue(container, null);
                    if (value is IQueryable queryable)
                    {
                        return ReplaceMemberExpression(queryable);
                    }

                    if (value is DbContext dbContext)
                    {
                        return ReplaceMemberExpression(dbContext);
                    }
                }
            }
            //}

            return base.VisitMember(memberExpression);
        }

        private MemberExpression ReplaceMemberExpression(IQueryable queryable)
        {
            var dbContextReplaceQueryableVisitor = new DbContextReplaceQueryableVisitor(_dbContext);
            var newExpression = dbContextReplaceQueryableVisitor.Visit(queryable.Expression);
            var newQueryable = dbContextReplaceQueryableVisitor.Source.Provider.CreateQuery(newExpression);
            var tempVariableGenericType = typeof(TempVariable<>).GetGenericType0(queryable.ElementType);
            var tempVariable = Activator.CreateInstance(tempVariableGenericType, newQueryable);
            MemberExpression queryableMemberReplaceExpression =
                Expression.Property(ConstantExpression.Constant(tempVariable), nameof(TempVariable<object>.Queryable));
            return queryableMemberReplaceExpression;
        }

        private MemberExpression ReplaceMemberExpression(DbContext dbContext)
        {
            var tempVariableGenericType = typeof(TempDbVariable<>).GetGenericType0(dbContext.GetType());
            var tempVariable = Activator.CreateInstance(tempVariableGenericType, _dbContext);
            MemberExpression dbContextMemberReplaceExpression =
                Expression.Property(ConstantExpression.Constant(tempVariable),
                    nameof(TempDbVariable<object>.DbContext));
            return dbContextMemberReplaceExpression;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (RootIsVisit&&node.Method.ReturnType.IsMethodReturnTypeQueryableType()&&node.Method.ReturnType.IsGenericType)
            {
                var notRoot = node.Arguments.IsEmpty();

                if (notRoot)
                {
                    var entityType = node.Method.ReturnType.GenericTypeArguments[0];

                    var whereCallExpression = ReplaceMethodCallExpression(node, entityType);
                    return whereCallExpression;
                }
            }

            return base.VisitMethodCall(node);
        }

        private MethodCallExpression ReplaceMethodCallExpression(MethodCallExpression methodCallExpression,
            Type entityType)
        {
            MethodCallExpression whereCallExpression = Expression.Call(
                typeof(IShardingQueryableExtension),
                nameof(IShardingQueryableExtension.ReplaceDbContextQueryableWithType),
                new Type[] { entityType },
                methodCallExpression, Expression.Constant(_dbContext)
            );
            return whereCallExpression;
        }

        internal sealed class TempVariable<T1>
        {
            public IQueryable<T1> Queryable { get; }

            public TempVariable(IQueryable<T1> queryable)
            {
                Queryable = queryable;
            }

            public IQueryable<T1> GetQueryable()
            {
                return Queryable;
            }
        }

        internal sealed class TempDbVariable<T1>
        {
            public T1 DbContext { get; }

            public TempDbVariable(T1 dbContext)
            {
                DbContext = dbContext;
            }
        }
    }

#if EFCORE2 || EFCORE3
    internal class DbContextReplaceQueryableVisitor : DbContextInnerMemberReferenceReplaceQueryableVisitor
    {
        private readonly DbContext _dbContext;
        public IQueryable Source;

        public DbContextReplaceQueryableVisitor(DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable)
            {
                var dbContextDependencies =
                    typeof(DbContext).GetTypePropertyValue(_dbContext, "DbContextDependencies") as
                        IDbContextDependencies;
                var targetIQ =
                    (IQueryable)((IDbSetCache)_dbContext).GetOrAddSet(dbContextDependencies.SetSource,
                        queryable.ElementType);
                IQueryable newQueryable = null;
                //if (_isParallelQuery)
                //    newQueryable = targetIQ.Provider.CreateQuery((Expression)Expression.Call((Expression)null, typeof(EntityFrameworkQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking)).MakeGenericMethod(queryable.ElementType), targetIQ.Expression));
                //else
                newQueryable = targetIQ.Provider.CreateQuery(targetIQ.Expression);
                if (Source == null)
                    Source = newQueryable;
                // return base.Visit(Expression.Constant(newQueryable));
                RootIsVisit = true;
                return Expression.Constant(newQueryable);
            }

            return base.VisitConstant(node);
        }


    }
#endif

#if EFCORE5 || EFCORE6
    internal class DbContextReplaceQueryableVisitor : DbContextInnerMemberReferenceReplaceQueryableVisitor
    {
        private readonly DbContext _dbContext;
        public IQueryable Source;

        public DbContextReplaceQueryableVisitor(DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is QueryRootExpression queryRootExpression)
            {
                var dbContextDependencies =
                    typeof(DbContext).GetTypePropertyValue(_dbContext, "DbContextDependencies") as
                        IDbContextDependencies;
                var targetIQ =
                    (IQueryable)((IDbSetCache)_dbContext).GetOrAddSet(dbContextDependencies.SetSource,
                        queryRootExpression.EntityType.ClrType);

                var newQueryable = targetIQ.Provider.CreateQuery(targetIQ.Expression);
                if (Source == null)
                    Source = newQueryable;
                //如何替换ef5的set
                var replaceQueryRoot = new ReplaceSingleQueryRootExpressionVisitor();
                replaceQueryRoot.Visit(newQueryable.Expression);
                RootIsVisit = true;
                return base.VisitExtension(replaceQueryRoot.QueryRootExpression);
            }

            return base.VisitExtension(node);
        }

        internal sealed class ReplaceSingleQueryRootExpressionVisitor : ExpressionVisitor
        {
            public QueryRootExpression QueryRootExpression { get; set; }

            protected override Expression VisitExtension(Expression node)
            {
                if (node is QueryRootExpression queryRootExpression)
                {
                    if (QueryRootExpression != null)
                        throw new ShardingCoreException("replace query root more than one query root");
                    QueryRootExpression = queryRootExpression;
                }

                return base.VisitExtension(node);
            }
        }
    }
#endif
}