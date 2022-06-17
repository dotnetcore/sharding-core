using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Bootstrappers;

namespace Samples.AutoByDate.SqlServer
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 26 January 2021 12:29:04
* @Email: 326308290@qq.com
*/
    public static class DIExtension
    {
        public static IApplicationBuilder UseShardingCore(this IApplicationBuilder app)
        {
            var shardingBootstrapper = app.ApplicationServices.GetRequiredService<IShardingBootstrapper>();
            shardingBootstrapper.Start();
            return app;
        }
    }
}