using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCoreTestSqlServer3x.Domain.Maps;

namespace ShardingCoreTestSqlServer3x
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/31 15:28:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultDbContext : DbContext, IShardingTableDbContext
    {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysUserSalaryMap());
        }

        public string ModelChangeKey { get; set; }
    }
}
