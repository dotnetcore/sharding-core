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
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var virtualDbContext = scope.ServiceProvider.GetService<DefaultShardingDbContext>();
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
                            Name = $"ds{(id % 3)}",
                        });
                    }

                    var userModMonths = new List<SysUserLogByMonth>();
                    var begin = new DateTime(2021, 1, 01);
                    foreach (var id in ids)
                    {
                        userModMonths.Add(new SysUserLogByMonth()
                        {
                            Id = id.ToString(),
                            Time = begin.AddHours(id * 12)
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

        public static void DbSeed1(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var virtualDbContext = scope.ServiceProvider.GetService<DefaultShardingDbContext>();
                if (!virtualDbContext.Set<GroupEntity>().Any())
                {
                    var userMods = new List<GroupEntity>();
                    {
                        userMods.Add(new GroupEntity()
                        {
                            Id = "1",
                            City = "郑州市",
                            Area = "商丘市",
                            UserCount = 1,
                        });
                    }
                    {
                        userMods.Add(new GroupEntity()
                        {
                            Id = "2",
                            City = "郑州市",
                            Area = "商丘市",
                            UserCount = 2,
                        });
                    }
                    {
                        userMods.Add(new GroupEntity()
                        {
                            Id = "3",
                            City = "郑州市",
                            Area = "南阳市",
                            UserCount = 3,
                        });
                    }
                    {
                        userMods.Add(new GroupEntity()
                        {
                            Id = "4",
                            City = "郑州市",
                            Area = "商丘市",
                            UserCount = 4,
                        });
                    }
                    {
                        userMods.Add(new GroupEntity()
                        {
                            Id = "5",
                            City = "郑州市",
                            Area = "南阳市",
                            UserCount = 5,
                        });
                    }
                    {
                        userMods.Add(new GroupEntity()
                        {
                            Id = "6",
                            City = "郑州市",
                            Area = "太原市",
                            UserCount = 6,
                        });
                    }
                    virtualDbContext.AddRange(userMods);
                    virtualDbContext.SaveChanges();
                }
            }
        }
    }
}