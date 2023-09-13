using Sample.MySql.DbContexts;

namespace Sample.MySql.Controllers;

public class AsyncBatchService
{
    private readonly DefaultShardingDbContext _dbContext;

    public AsyncBatchService(DefaultShardingDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public void SaveInOtherThread(List<object> list)
    {
        _dbContext.AddRangeAsync(list);
        _dbContext.SaveChanges();
    }
}