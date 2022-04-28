using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators.AggregateExtensions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.MergeEngines.AggregateMergeEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:15:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AverageAsyncInMemoryMergeEngine<TEntity, TResult,TSelect> : AbstractNoTripEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {
        public AverageAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        private async Task<List<RouteQueryResult<AverageResult<T>>>> AggregateAverageResultAsync<T>(CancellationToken cancellationToken = new CancellationToken())
        {
            return (await base.ExecuteAsync(
                async queryable =>
                {
                    var count = await ((IQueryable<T>)queryable).LongCountAsync(cancellationToken);
                    if (count <= 0)
                    {
                        return default;
                    }

                    var sum = await GetSumAsync<T>(queryable, cancellationToken);
                    return new AverageResult<T>(sum, count);

                },
                cancellationToken)).Where(o => o.QueryResult != null).ToList();
            //            return (await base.ExecuteAsync(
            //                async queryable =>
            //                {
            //                    var count = 0L;
            //                    T sum = default;
            //                    //MethodInfo sumMethod = typeof(Queryable).GetMethods().First(
            //                    //    m => m.Name == nameof(Queryable.Sum)
            //                    //         && m.ReturnType == typeof(T)
            //                    //         && m.IsGenericMethod);

            //                    //var genericSumMethod = sumMethod.MakeGenericMethod(new[] { source.ElementType });
            //                    var newQueryable = ((IQueryable<T>)queryable).Select(o=>(decimal?)(object)o);
            //#if !EFCORE2
            //                    var r = await newQueryable.GroupBy(o=>1).Select(o=>new
            //                    {
            //                        C= o.LongCount(),
            //                        //S = ShardingEntityFrameworkQueryableExtensions.Execute<T,T>(ShardingQueryableMethods.GetSumWithoutSelector(typeof(T)), newQueryable, (Expression)null)
            //                        S = o.Sum()

            //                    }).FirstOrDefaultAsync(cancellationToken);
            //                ////https://stackoverflow.com/questions/21143179/build-groupby-expression-tree-with-multiple-fields
            //                ////https://blog.wiseowls.co.nz/index.php/2021/05/13/ef-core-3-1-dynamic-groupby-clause/


            //                ////https://blog.wiseowls.co.nz/index.php/2021/05/13/ef-core-3-1-dynamic-groupby-clause/
            //                ////https://stackoverflow.com/questions/39728898/groupby-query-by-linq-expressions-and-lambdas
            //                //    Expression.New(
            //                //        Type.GetType("System.Tuple`" + fields.Length)
            //                //            .MakeGenericType(fields.Select(studentType.GetProperty),
            //                //                fields.Select(f => Expression.PropertyOrField(itemParam, f))
            //                //            )
            //                //    if (r != null)
            //                //    {
            //                //        count = r.C;
            //                //        //sum = r.S;
            //                //    }

            //#endif
            //#if EFCORE2
            //                     count = await ((IQueryable<T>)queryable).LongCountAsync(cancellationToken);
            //                    if (count <= 0)
            //                    {
            //                        return default;
            //                    }

            //                    sum = await GetSumAsync<T>(queryable, cancellationToken);
            //#endif

            //                    return new AverageResult<T>(sum, count);

            //                },
            //                cancellationToken)).Where(o => o.QueryResult != null).ToList();
        }
        
        private async Task<T> GetSumAsync<T>(IQueryable queryable,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var resultType = typeof(T);
            if (!resultType.IsNumericType())
                throw new ShardingCoreException(
                    $"not support {GetStreamMergeContext().MergeQueryCompilerContext.GetQueryExpression().ShardingPrint()} result {resultType}");
#if !EFCORE2
            return await ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<T, Task<T>>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<T>)queryable, (Expression)null, cancellationToken);
#endif
#if EFCORE2
            return await ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<T, T>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<T>)queryable, cancellationToken);
#endif
            
        }
        public override async Task<TResult> MergeResultAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            {
                if (!typeof(TSelect).IsNumericType())
                {
                    throw new ShardingCoreException(
                        $"not support {GetStreamMergeContext().MergeQueryCompilerContext.GetQueryExpression().ShardingPrint()} result {typeof(TSelect)}");
                }
                var result = await AggregateAverageResultAsync<TSelect>(cancellationToken);
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var queryable = result.Select(o => new
                {
                    Sum = o.QueryResult.Sum,
                    Count = o.QueryResult.Count
                }).AsQueryable();
                var sum = queryable.SumByPropertyName<TSelect>(nameof(AverageResult<object>.Sum));
                var count = queryable.Sum(o => o.Count);
                return AggregateExtension.AverageConstant<TSelect, long, TResult>(sum, count);
            }
            
        }

    }
}