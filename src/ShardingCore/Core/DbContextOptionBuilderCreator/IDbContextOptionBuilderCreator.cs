using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Core.DbContextOptionBuilderCreator
{
    public interface IDbContextOptionBuilderCreator
    {
        DbContextOptionsBuilder CreateDbContextOptionBuilder();
    }
}
