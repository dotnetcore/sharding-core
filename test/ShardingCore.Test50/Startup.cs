using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;
using ShardingCore.SqlServer;
using ShardingCore.Test50.Domain.Entities;
using ShardingCore.Test50.Shardings;

namespace ShardingCore.Test50
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 15 January 2021 15:37:46
* @Email: 326308290@qq.com
*/
    public class Startup
    {
        // 自定义 host 构建
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("Configs/DbConfig.json");
                });
        }

        // 支持的形式：
        // ConfigureServices(IServiceCollection services)
        // ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        // ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
        {
            services.AddShardingSqlServer(o =>
            {
                o.ConnectionString = hostBuilderContext.Configuration.GetSection("SqlServer")["ConnectionString"];
                o.AddSharding<SysUserModVirtualRoute>();
                o.AddSharding<SysUserRangeVirtualRoute>();
                o.EnsureCreated = true;
            });
        }

        // 可以添加要用到的方法参数，会自动从注册的服务中获取服务实例，类似于 asp.net core 里 Configure 方法
        public void Configure(IServiceProvider serviceProvider)
        {
            var shardingBootstrapper = serviceProvider.GetService<ShardingBootstrapper>();
            shardingBootstrapper.Start();
            // 有一些测试数据要初始化可以放在这里
           InitData(serviceProvider).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 添加种子数据
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private  async Task InitData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var virtualDbContext = scope.ServiceProvider.GetService<IVirtualDbContext>();
                if (!await virtualDbContext.Set<SysUserMod>().ShardingAnyAsync(o => true))
                {
                    var ids = Enumerable.Range(1, 100);
                    var userMods = new List<SysUserMod>();
                    foreach (var id in ids)
                    {
                        userMods.Add(new SysUserMod()
                        {
                            Id = id.ToString(),
                            Age = id,
                            Name = $"name_{id}"
                        });
                    }

                    await virtualDbContext.InsertRangeAsync(userMods);
                    
                    
                    var idRanges = Enumerable.Range(1, 1000);
                    var userRanges = new List<SysUserRange>();
                    foreach (var id in idRanges)
                    {
                        userRanges.Add(new SysUserRange()
                        {
                            Id = id.ToString(),
                            Age = id,
                            Name = $"name_range_{id}"
                        });
                    }

                    await virtualDbContext.InsertRangeAsync(userRanges);
                    await virtualDbContext.SaveChangesAsync();
                }
            }
        }
    }
}