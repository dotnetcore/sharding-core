using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 09:57:08
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分表分库的dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ShardingDbContext<T> : DbContext, IShardingDbContext where T : DbContext
    {
        private readonly string EMPTY_SHARDING_TAIL_ID = Guid.NewGuid().ToString("n");
        private readonly ConcurrentDictionary<string, DbContext> _dbContextCaches = new ConcurrentDictionary<string, DbContext>();
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        public ShardingDbContext(DbContextOptions options):base(options)
        {
            _shardingDbContextFactory = ShardingContainer.GetService<IShardingDbContextFactory>();
        }
        public DbContext GetDbContext(bool track, string tail)
        {
            if (!_dbContextCaches.TryGetValue(tail, out var dbContext))
            {
                dbContext = _shardingDbContextFactory.Create(track ? this.Database.GetDbConnection() : null, tail == EMPTY_SHARDING_TAIL_ID ? string.Empty : tail); ;
                _dbContextCaches.TryAdd(tail, dbContext);
            }

            //if (IsOpenTransaction)
            //{
            //    _dbTransaction.Use(dbContext);
            //}

            return dbContext;
        }
        public DbContext CreateGenericDbContext<T>(T entity) where T : class
        {
            var tail = EMPTY_SHARDING_TAIL_ID;
            if (entity.IsShardingTable())
            {
                var physicTable = _virtualTableManager.GetVirtualTable(entity.GetType()).RouteTo(new TableRouteConfig(null, entity as IShardingTable, null))[0];
                tail = physicTable.Tail;
            }

            return GetDbContext(true,tail);
        }

    }
}