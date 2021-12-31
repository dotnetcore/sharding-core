using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sample.SqlServerShardingTable.Entities;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.SqlServerShardingTable
{
    public class MyDbContext:AbstractShardingDbContext,IShardingTableDbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.Property(o=>o.Payer).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.Property(o => o.Area).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.Property(o => o.OrderStatus).HasConversion<int>();
                entity.ToTable(nameof(Order));
            });
            modelBuilder.Entity<SysUser>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.Property(o=>o.Name).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.Property(o=>o.Area).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.Property(o => o.SettingCode).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.ToTable(nameof(SysUser));
            });
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(o => o.Code);
                entity.Property(o => o.Code).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.Property(o=>o.Name).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.ToTable(nameof(Setting));
            });
            modelBuilder.Entity<MultiShardingOrder>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).ValueGeneratedNever();
                entity.Property(o=>o.Name).IsRequired().IsUnicode(false).HasMaxLength(50);
                entity.HasQueryFilter(o => o.IsDelete == false);
                entity.ToTable(nameof(MultiShardingOrder));
            });
        }
        /// <summary>
        /// empty impl
        /// </summary>
        public IRouteTail RouteTail { get; set; }

        public override void Dispose()
        {
            Console.WriteLine("MyDbContext disposed");
            base.Dispose();
        }

        public override ValueTask DisposeAsync()
        {
            Console.WriteLine("MyDbContext disposed async");
            return base.DisposeAsync();
        }
    }
}
