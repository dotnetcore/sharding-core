using System.Linq.Expressions;

namespace ShardingCore.CommonTest;

public static class QueryableExtension
{
    public static IQueryable<T> CheckBetween<T, S>(this IQueryable<T> baseQuery, S? valMin,S? valMax,Expression<Func<T,S?>> field,bool includeMin=true,bool includeMax=true) where S : struct
    {
        
        if (valMin == null && valMax == null)
        {
            return baseQuery;
        }
        else
        {
            IQueryable<T> rv = baseQuery;
            if (valMin != null)
            {
                BinaryExpression exp1 = !includeMin ? Expression.GreaterThan(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(valMin)) : Expression.GreaterThanOrEqual(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(valMin));
                rv = rv.Where(Expression.Lambda<Func<T, bool>>(exp1, field.Parameters[0]));
            }
            if (valMax != null)
            {
                BinaryExpression exp2 = !includeMax ? Expression.LessThan(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(valMax)) : Expression.LessThanOrEqual(Expression.PropertyOrField(field.Body, "Value"), Expression.Constant(valMax));
                rv = rv.Where(Expression.Lambda<Func<T, bool>>(exp2, field.Parameters[0]));
            }
            return rv;
        }
    }
}