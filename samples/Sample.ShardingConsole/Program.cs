// See https://aka.ms/new-console-template for more information

using Microsoft.EntityFrameworkCore;
using Sample.ShardingConsole;
using ShardingCore;
using ShardingCore.Extensions;

ShardingProvider.ShardingRuntimeContext.UseAutoShardingCreate();
ShardingProvider.ShardingRuntimeContext.UseAutoTryCompensateTable();

var dbContextOptionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
dbContextOptionsBuilder.UseDefaultSharding<MyDbContext>(ShardingProvider.ShardingRuntimeContext);
using (var dbcontext = new MyDbContext(dbContextOptionsBuilder.Options))
{
    dbcontext.Add(new Order()
    {
        Id = Guid.NewGuid().ToString("n"),
        Payer = "111",
        Area = "123",
        OrderStatus = OrderStatusEnum.Payed,
        Money = 100,
        CreationTime = DateTime.Now
    });
    dbcontext.SaveChanges();
}

Console.WriteLine("Hello, World!");