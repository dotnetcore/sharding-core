using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Sample.MySql
{

    public class MySaveChangeInterceptor:SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var myCurrentUser = eventData.Context.GetService<MyCurrentUser>();
            Console.WriteLine("1"+myCurrentUser!=null);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}