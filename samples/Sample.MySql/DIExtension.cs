using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sample.MySql.DbContexts;
using Sample.MySql.Domain.Entities;
using ShardingCore;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;

namespace Sample.MySql
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

        public static void DbSeed(this IApplicationBuilder app)
        {
            using (var scope=app.ApplicationServices.CreateScope())
            {
                var virtualDbContext =scope.ServiceProvider.GetService<DefaultTableDbContext>();
                if (!virtualDbContext.Set<SysUserMod>().ShardingAny())
                {
                    var ids = Enumerable.Range(1, 1000);
                    var userMods = new List<SysUserMod>();
                    foreach (var id in ids)
                    {
                        userMods.Add(new SysUserMod()
                        {
                            Id = id.ToString(),
                            Age = id,
                            Name = $"name_{id}",
                        });
                    }
                    var userModMonths = new List<SysUserLogByMonth>();
                    foreach (var id in ids)
                    {
                        userModMonths.Add(new SysUserLogByMonth()
                        {
                            Id = id.ToString(),
                            Time = DateTime.Now
                        });
                    }

                    virtualDbContext.AddRange(userMods);
                    virtualDbContext.AddRange(userModMonths);
                    virtualDbContext.SaveChanges();
                }
            }
        }
    }
}