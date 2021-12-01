using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore6x.NoShardingDbContexts
{
    internal class DefaultDbContext:DbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options):base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OrderMap());
        }
    }
}
