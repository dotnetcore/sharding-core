using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.NoShardingMultiLevel.Entities;

namespace Sample.NoShardingMultiLevel.Maps
{
    public class DepartmentMap : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Name).IsRequired().HasMaxLength(50);
            builder.Property(o => o.CompanyId).IsRequired().HasMaxLength(50);
            builder.HasOne(o => o.Company).WithMany(o => o.Departments).HasForeignKey(o => o.CompanyId);
            builder.ToTable(nameof(Department));
        }
    }
}