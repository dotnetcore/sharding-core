using Sample.MySql.DbContexts;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Extensions;

namespace Sample.MySql
{

    public class NoReadWriteMiddleware
    {
        private readonly RequestDelegate _next;

        public NoReadWriteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.ToString().StartsWith("/admin"))
            {
                var shardingRuntimeContext = context.RequestServices.GetService<IShardingRuntimeContext<DefaultShardingDbContext>>();
                var shardingReadWriteManager = shardingRuntimeContext.GetShardingReadWriteManager();
                using (var scope = shardingReadWriteManager.CreateScope())
                {
                    shardingReadWriteManager.GetCurrent().SetReadWriteSeparation(9999,false);
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
            
        }
    
    }
}