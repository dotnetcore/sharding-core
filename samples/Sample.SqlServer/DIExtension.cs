using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sample.SqlServer.DbContexts;
using Sample.SqlServer.Domain.Entities;
using ShardingCore;
using ShardingCore.Bootstrappers;

namespace Sample.SqlServer
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
                if (!virtualDbContext.Set<SysUserMod>().Any(o=>o.Id=="111"))
                {
                    var ids = Enumerable.Range(1, 1000);
                    var userMods = new List<SysUserMod>();
                    var userSalaries = new List<SysUserSalary>();
                    var SysTests = new List<SysTest>();
                    var beginTime = new DateTime(2020, 1, 1);
                    var endTime = new DateTime(2021, 12, 1);
                    foreach (var id in ids)
                    {
                        userMods.Add(new SysUserMod()
                        {
                            Id = id.ToString(),
                            Age = id,
                            Name = $"name_{id}",
                            AgeGroup = Math.Abs(id % 10)
                        });
                        SysTests.Add(new SysTest()
                        {
                            Id = id.ToString(),
                            UserId = id.ToString()
                        });
                        var tempTime = beginTime;
                        var i = 0;
                        while (tempTime <= endTime)
                        {
                            var dateOfMonth = $@"{tempTime:yyyyMM}";
                            userSalaries.Add(new SysUserSalary()
                            {
                                Id = $@"{id}{dateOfMonth}",
                                UserId = id.ToString(),
                                DateOfMonth = int.Parse(dateOfMonth),
                                Salary = 700000 + id * 100 * i,
                                SalaryLong = 700000 + id * 100 * i,
                                SalaryDecimal = (700000 + id * 100 * i) / 100m,
                                SalaryDouble = (700000 + id * 100 * i) / 100d,
                                SalaryFloat = (700000 + id * 100 * i) / 100f
                            });
                            tempTime = tempTime.AddMonths(1);
                            i++;
                        }
                    }



                    using (var tran = virtualDbContext.Database.BeginTransaction())
                    {
                        virtualDbContext.AddRange(userMods);
                        virtualDbContext.AddRange(SysTests);
                        virtualDbContext.AddRange(userSalaries);

                        virtualDbContext.SaveChanges();
                        tran.Commit();
                    }
                }
            }
        }
    }
}