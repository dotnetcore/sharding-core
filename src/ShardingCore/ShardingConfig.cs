using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/16 15:18:37
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingConfig<T> where T:DbContext,IShardingTableDbContext
    {

        public Func<DbConnection, DbContextOptionsBuilder<T>, DbContextOptions<T>> ShardingDbContextOptionsCreator { get; private set; }
        public void UseShardingDbContextOptions(Func<DbConnection, DbContextOptionsBuilder<T>, DbContextOptions<T>> shardingDbContextOptions)
        {
            ShardingDbContextOptionsCreator = shardingDbContextOptions ?? throw new ArgumentNullException(nameof(shardingDbContextOptions));
        }
    }
}
