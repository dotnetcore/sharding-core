using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding;
using ShardingCore.Test50.Domain.Maps;

namespace ShardingCore.Test50
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 10:21:03
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDefaultDbContext:ShardingDbContext<DefaultDbContext>
    {
        public ShardingDefaultDbContext(DbContextOptions<ShardingDefaultDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysUserSalaryMap());
        }
    }
}
