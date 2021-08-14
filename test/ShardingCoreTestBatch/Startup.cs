// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using ShardingCore;
// using ShardingCore.DbContexts.VirtualDbContexts;
// using ShardingCore.Extensions;
// using ShardingCore.SqlServer;
// using ShardingCoreTestBatch.Domain.Entities;
// using ShardingCoreTestBatch.Shardings;
//
// #if EFCORE5SQLSERVER
// using ShardingCore.SqlServer;
// #endif
// #if EFCORE5MYSQL
// using ShardingCore.MySql;
// #endif
//
// namespace ShardingCoreTestBatch
// {
// /*
// * @Author: xjm
// * @Description:
// * @Date: Friday, 15 January 2021 15:37:46
// * @Email: 326308290@qq.com
// */
//     public class Startup
//     {
//         // // 自定义 host 构建
//         //public void ConfigureHost(IHostBuilder hostBuilder)
//         //{
//         //    hostBuilder
//         //        .ConfigureAppConfiguration(builder =>
//         //        {
//         //            builder.AddJsonFile("Configs/DbConfig.json");
//         //        });
//         //}
//
//         // 支持的形式：
//         // ConfigureServices(IServiceCollection services)
//         // ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
//         // ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
//         public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
//         {
//
//             services.AddShardingSqlServer(o =>
//             {
//                 o.EnsureCreatedWithOutShardingTable = true;
//                 o.CreateShardingTableOnStart = true;
//                 o.UseShardingDbContext<DefaultDbContext>(dbConfig =>
//                 {
//                     dbConfig.AddShardingTableRoute<SysUserModVirtualTableRoute>();
//                 });
//                 //o.AddDataSourceVirtualRoute<>();
//
//             });
//             //services.AddShardingSqlServer(o =>
//             //{
//             //    o.ConnectionString = hostBuilderContext.Configuration.GetSection("SqlServer")["ConnectionString"];
//             //    o.AddSharding<SysUserModVirtualTableRoute>();
//             //    o.AddSharding<SysUserSalaryVirtualTableRoute>();
//             //    o.UseShardingCoreConfig((provider, config) =>
//             //    {
//             //        config.EnsureCreated = true;
//             //        config.CreateShardingTableOnStart = true;
//             //    });
//             //});
//             
//             services.AddDbContext<DefaultDbContext>(o=>o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBBatch;Integrated Security=True"));
//         }
//
//         // 可以添加要用到的方法参数，会自动从注册的服务中获取服务实例，类似于 asp.net core 里 Configure 方法
//         public void Configure(IServiceProvider serviceProvider)
//         {
//             var shardingBootstrapper = serviceProvider.GetService<IShardingBootstrapper>();
//             shardingBootstrapper.Start();
//             // 有一些测试数据要初始化可以放在这里
//            InitData(serviceProvider).GetAwaiter().GetResult();
//         }
//
//         /// <summary>
//         /// 添加种子数据
//         /// </summary>
//         /// <param name="serviceProvider"></param>
//         /// <returns></returns>
//         private  async Task InitData(IServiceProvider serviceProvider)
//         {
//             using (var scope = serviceProvider.CreateScope())
//             {
//                 var virtualDbContext = scope.ServiceProvider.GetService<DefaultDbContext>();
//                 if (!await virtualDbContext.Set<SysUserMod>().ShardingAnyAsync(o => true))
//                 {
//                     var ids = Enumerable.Range(1, 9000000);
//                     var userMods = new List<SysUserMod>();
//                     foreach (var id in ids)
//                     {
//                         userMods.Add(new SysUserMod()
//                         {
//                             Id = id.ToString(),
//                             Age = id,
//                             Name = $"name_{id}",
//                             AgeGroup=Math.Abs(id%10)
//                         });
//                       
//                     }
//                     //
//                     // var shardingBatchInsertEntry = virtualDbContext.BulkInsert(userMods);
//                     // shardingBatchInsertEntry.BatchGroups.ForEach(g =>
//                     // {
//                     //     g.Key.BulkInsert(g.Value);
//                     // });
//                     
//                     await virtualDbContext.SaveChangesAsync();
//                 }
//             }
//         }
//     }
// }