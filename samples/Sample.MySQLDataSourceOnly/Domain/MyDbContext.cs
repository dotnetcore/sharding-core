using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding;

namespace Sample.MySQLDataSourceOnly.Domain;


public class MyDbContext : AbstractShardingDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).IsRequired().IsUnicode(false).HasMaxLength(50);
            entity.Property(o => o.Name).IsRequired().IsUnicode(false).HasMaxLength(50);
            entity.Property(o => o.Area).IsRequired().IsUnicode(false).HasMaxLength(50);
            entity.ToTable(nameof(SysUser));
        });
    }
}