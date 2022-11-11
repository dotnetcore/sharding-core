using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
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
    }
}