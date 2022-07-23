using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Extensions;

namespace Sample.MySql.Controllers;

public class DbSetDiscoverExpressionVisitor<TEntity>:ExpressionVisitor where TEntity:class
{
    private readonly DbContext _dbContext;
    public DbSet<TEntity> DbSet { get; private set; }

    public DbSetDiscoverExpressionVisitor(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override Expression VisitExtension(Expression node)
    {
        if (node is QueryRootExpression queryRootExpression)
        {
            var dbContextDependencies =
                typeof(DbContext).GetTypePropertyValue(_dbContext, "DbContextDependencies") as IDbContextDependencies;
            var targetIQ =
                ((IDbSetCache)_dbContext).GetOrAddSet(dbContextDependencies.SetSource, queryRootExpression.EntityType.ClrType);
            DbSet = (DbSet<TEntity>)targetIQ;
        }
        return base.VisitExtension(node);
    }
}