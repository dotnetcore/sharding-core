// using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
//
// namespace Sample.MultiConfig.Middlewares
// {
//     public class TenantSelectMiddleware
//     {
//         private readonly RequestDelegate _next;
//         private readonly IVirtualDataSourceManager<MultiConfigDbContext> _virtualDataSourceManager;
//
//         public TenantSelectMiddleware(RequestDelegate next, IVirtualDataSourceManager<MultiConfigDbContext> virtualDataSourceManager)
//         {
//             _next = next;
//             _virtualDataSourceManager = virtualDataSourceManager;
//         }
//
//         /// <summary>
//         /// 1.中间件的方法必须叫Invoke，且为public，非static。
//         /// 2.Invoke方法第一个参数必须是HttpContext类型。
//         /// 3.Invoke方法必须返回Task。
//         /// 4.Invoke方法可以有多个参数，除HttpContext外其它参数会尝试从依赖注入容器中获取。
//         /// 5.Invoke方法不能有重载。
//         /// </summary>
//         /// Author : Napoleon
//         /// Created : 2020/1/30 21:30
//         public async Task Invoke(HttpContext context)
//         {
//             if (context.Request.Path.ToString().StartsWith("/test", StringComparison.CurrentCultureIgnoreCase))
//             {
//                 if (!context.Request.Headers.ContainsKey("TenantId") || !_virtualDataSourceManager.ContansConfigId(context.Request.Headers["TenantId"]))
//                 {
//                     context.Response.StatusCode = 403; //UnAuthorized
//                     await context.Response.WriteAsync("403 not found TenantId");
//                     return;
//                 }
//
//                 using (_virtualDataSourceManager.CreateScope(context.Request.Headers["TenantId"]))
//                 {
//                     await _next(context);
//                 }
//             }
//             else
//             {
//                 await _next(context);
//             }
//         }
//     }
// }
