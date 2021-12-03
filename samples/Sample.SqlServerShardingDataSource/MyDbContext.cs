using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Namotion.Reflection;
using Sample.SqlServerShardingDataSource.Entities;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.SqlServerShardingDataSource
{
    public class MyDbContext:AbstractShardingDbContext
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
                entity.ToTable(nameof(SysUser));
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.PropertyInfo == null)
                        continue;
                    var x = property.GetComment();
                    if (!string.IsNullOrWhiteSpace(property.GetComment()))
                        continue;
                    StringBuilder comment = new StringBuilder(property.PropertyInfo.GetXmlDocsSummary());

                    if (property.PropertyInfo.PropertyType.IsEnum)
                    {
                        foreach (var aValue in Enum.GetValues(property.PropertyInfo.PropertyType))
                        {
                            var member = property.PropertyInfo.PropertyType.GetMembers()
                                .Where(x => x.Name == aValue.ToString())
                                .FirstOrDefault();
                            var memberComment = member?.GetXmlDocsSummary();
                            if (string.IsNullOrWhiteSpace(memberComment))
                                memberComment = member?.Name;
                            comment.Append($" {(int)aValue}={memberComment}");
                        }
                    }
                    property.SetComment(comment.ToString());
                }
            }
        }
    }
}
