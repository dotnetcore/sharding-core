using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.MySql.multi;

public class OtherDbContext:AbstractShardingDbContext,IShardingTableDbContext
{
    public DbSet<MyUser> MyUsers { get; set; }
    public OtherDbContext(DbContextOptions<OtherDbContext> options) : base(options)
    {
    }

    public static string CurrentId;
    public string CID => CurrentId;
    public bool HasCID => !string.IsNullOrWhiteSpace(CID);
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MyUser>()
            .HasQueryFilter(o => !HasCID || o.Id == CID);
    }

    public IRouteTail RouteTail { get; set; }
}