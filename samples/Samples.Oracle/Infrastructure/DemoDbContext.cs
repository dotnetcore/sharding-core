using Microsoft.EntityFrameworkCore;
using Samples.Oracle.Domain;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Samples.Oracle.Infrastructure;

public class DemoDbContext : AbstractShardingDbContext, IShardingTableDbContext
{
    public DemoDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Student>(table =>
        {
            table.HasKey(e => e.Id);

            table.Property(e => e.Id)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(36)
                .ValueGeneratedNever();

            table.Property(e => e.Name)
                .IsRequired()
                .IsUnicode(true)
                .HasMaxLength(36);

            table.Property(e => e.CreationTime)
                .IsRequired();

            table.ToTable(nameof(Student));
        });
    }

    public virtual IRouteTail RouteTail { get; set; }

    public virtual DbSet<Student> Student { get; set; }
}
