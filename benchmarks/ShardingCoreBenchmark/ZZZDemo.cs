using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.EFCores;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore6x.ShardingDbContexts;
using Z.EntityFramework.Plus;

namespace ShardingCore6x
{
    public class ZZZDemo
    {
        private readonly List<Order> _orders = new List<Order>();
        private readonly DefaultShardingDbContext _dbcontext;

        public async Task BulkInsertAsync()
        {
            // dbcontext.BulkShardingEnumerable(orders);//如果有分库用这个
            var orderWithShardingGroup = _dbcontext.BulkShardingTableEnumerable(_orders);
            using (var tran = await _dbcontext.Database.BeginTransactionAsync())
            {
                foreach (var orderWithDbContext in orderWithShardingGroup)
                {
                    await orderWithDbContext.Key.BulkInsertAsync(orderWithDbContext.Value);
                }

                await tran.CommitAsync();
            }
        }

        public async Task BulkUpdateAsync()
        {
            // dbcontext.BulkShardingExpression(orders);//如果有分库用这个
            Expression<Func<Order, bool>> where = o => o.Id == "123";
            var dbContexts = _dbcontext.BulkShardingTableExpression(where);
            using (var tran = await _dbcontext.Database.BeginTransactionAsync())
            {
                foreach (var dbContext in dbContexts)
                {
                    await dbContext.Set<Order>().Where(where).UpdateAsync(o => new Order()
                    {
                        Body = o.Body + "123"
                    });
                }

                await tran.CommitAsync();
            }
        }
        public async Task BulkUpdateAsync2()
        {
            await _dbcontext.ShardingBulkUpdateAsync<Order>(o => o.Id == "123", o => new Order()
            {
                Body = o.Body + "123"
            });
        }
    }

    internal static class ZZZExtension
    {
        public static async Task<int> ShardingBulkUpdateAsync<T>(this DefaultShardingDbContext shardingDbContext, Expression<Func<T, bool>> where,
            Expression<Func<T, T>> updateFactory,
            CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            var dbContexts = shardingDbContext.BulkShardingTableExpression(where);
            var effectRows = 0;
            foreach (var dbContext in dbContexts)
            {
                effectRows += await dbContext.Set<T>().Where(where)
                    .UpdateAsync(updateFactory, cancellationToken: cancellationToken);
            }

            return effectRows;
        }
        // public static async Task<int> ShardingBulkUpdateAsync2<T>(this IQueryable<T> queryable,
        //     Expression<Func<T, T>> updateFactory,
        //     CancellationToken cancellationToken = default(CancellationToken)) where T : class
        // {
        //     var shardingDbContext = GetShardingDbContext(queryable);
        //     var shardingRuntimeContext = ((DbContext)shardingDbContext).GetShardingRuntimeContext();
        //     var tableRouteManager = shardingRuntimeContext.GetTableRouteManager();
        //     var routeTailFactory = shardingRuntimeContext.GetRouteTailFactory();
        //     var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
        //     var tableRouteUnits = tableRouteManager.RouteTo(typeof(T),virtualDataSource.DefaultDataSourceName,new ShardingTableRouteConfig(queryable:queryable));
        //     tableRouteUnits.Select(tableRouteUnit =>
        //     {
        //         var dbContext = shardingDbContext.GetDbContext(tableRouteUnit.DataSourceName,
        //             CreateDbContextStrategyEnum.ShareConnection, routeTailFactory.Create(tableRouteUnit.Tail));
        //         queryable.Expression.re
        //     })
        //     var dbContexts = shardingDbContext.BulkShardingTableExpression(where);
        //     var effectRows = 0;
        //     foreach (var dbContext in dbContexts)
        //     {
        //         effectRows += await dbContext.Set<T>().Where(where)
        //             .UpdateAsync(updateFactory, cancellationToken: cancellationToken);
        //     }
        //
        //     return effectRows;
        // }
        // private static IShardingDbContext GetShardingDbContext<T>(IQueryable<T> source)
        // {
        //     
        //     var entityQueryProvider = source.Provider as EntityQueryProvider??throw new ShardingCoreInvalidOperationException($"cant use sharding page that {nameof(IQueryable)} provider not {nameof(EntityQueryProvider)}");
        //
        //     var shardingQueryCompiler = ObjectExtension.GetFieldValue(entityQueryProvider,"_queryCompiler") as ShardingQueryCompiler??throw new ShardingCoreInvalidOperationException($"cant use sharding page that {nameof(EntityQueryProvider)} not contains {nameof(ShardingQueryCompiler)} filed named _queryCompiler");
        //     var dbContextAvailable = shardingQueryCompiler as IShardingDbContextAvailable;
        //     if (dbContextAvailable == null)
        //     {
        //         throw new ShardingCoreInvalidOperationException($"cant use sharding page that {nameof(ShardingQueryCompiler)} not impl  {nameof(IShardingDbContextAvailable)}");
        //     }
        //
        //     return dbContextAvailable.GetShardingDbContext();
        // }
    }
}