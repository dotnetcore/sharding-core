using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using ShardingCore.Core.Internal;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
   
    public class ShardingLocalView<TEntity>:LocalView<TEntity>  where TEntity : class
    {
        private readonly DbContext _dbContext;

        public ShardingLocalView(DbSet<TEntity> set) : base(set)
        {
            _dbContext = set.GetService<ICurrentDbContext>().Context;
        }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            if (_dbContext is ICurrentDbContextDiscover currentDbContextDiscover)
            {
                var dataSourceDbContexts = currentDbContextDiscover.GetCurrentDbContexts();
                var enumerators = dataSourceDbContexts.SelectMany(o => o.Value.GetCurrentContexts().Select(cd=>cd.Value.Set<TEntity>().Local.GetEnumerator()));
                return new MultiEnumerator<TEntity>(enumerators);
            }
            return base.GetEnumerator();
        }
    } 
}
