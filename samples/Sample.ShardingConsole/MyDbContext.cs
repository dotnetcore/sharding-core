using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.ShardingConsole;

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
    }
    /// <summary>
    /// empty impl if use sharding table
    /// </summary>
    public IRouteTail RouteTail { get; set; }
}