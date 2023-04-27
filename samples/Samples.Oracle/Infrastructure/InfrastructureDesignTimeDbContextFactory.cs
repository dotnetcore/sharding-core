using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Samples.Oracle.Infrastructure;

public class InfrastructureDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DemoDbContext>
{
    public DemoDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<DemoDbContext> builder = new();
        builder.UseOracle("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=xe)));User Id=SYSTEM;Password=oracle;", builder => builder.MigrationsAssembly("Samples.Oracle"));
        return new(builder.Options);
    }
}
