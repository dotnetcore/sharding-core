using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sample.MySql.DbContexts;
using Sample.MySql.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Sample.MySql.multi;
using ShardingCore.Bootstrappers;

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
        public static void DbSeed(this IApplicationBuilder app)
        {
            using (var scope=app.ApplicationServices.CreateScope())
            {
                var virtualDbContext =scope.ServiceProvider.GetService<DefaultShardingDbContext>();
                if (!virtualDbContext.Set<SysUserMod>().Any())
                {
                    var ids = Enumerable.Range(1, 1000);
                    var userMods = new List<SysUserMod>();
                    foreach (var id in ids)
                    {
                        userMods.Add(new SysUserMod()
                        {
                            Id = id.ToString(),
                            Age = id,
                            Name = $"ds{(id%3)}",
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
            
            // using (var scope=app.ApplicationServices.CreateScope())
            // {
            //   
            //     var virtualDbContext =scope.ServiceProvider.GetService<DefaultShardingDbContext>();
            //     var any = virtualDbContext.Set<SysUserMod>().Any();
            // }
            //using (var scope = app.ApplicationServices.CreateScope())
            //{
            //    var dbContext = scope.ServiceProvider.GetService<DefaultShardingDbContext>();
            //    var queryable = from sum in dbContext.Set<SysUserMod>()
            //        join st in dbContext.Set<SysTest>().Where(p =>  p.UserId == "admin")
            //        on sum.Id equals st.Id
            //        select new
            //        {
            //            st.UserId,
            //            sum.Name
            //        };
            //    var result =  queryable.ToList();

            //    Console.WriteLine(result.Count);
            //}

            //using (var scope = app.ApplicationServices.CreateScope())
            //{
            //    var dbContext = scope.ServiceProvider.GetService<DefaultShardingDbContext>();

            //    var queryable = from sum in dbContext.Set<SysUserMod>()
            //        from st in dbContext.Set<SysTest>().Where(p => p.Id == sum.Id && p.UserId == "admin")
            //        select new
            //        {
            //            st.UserId,
            //            sum.Name
            //        };
            //    var result =  queryable.ToList();

            //    Console.WriteLine(result.Count);
            //}




        }
    }
}