using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 10:32:26
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultConnectionStringManager<TShardingDbContext>:IConnectionStringManager where TShardingDbContext:DbContext,IShardingDbContext
    {
        public Type ShardingDbContextType => typeof(TShardingDbContext);

        public string GetConnectionString(IShardingDbContext shardingDbContext)
        {
            return shardingDbContext.GetConnectionString();
        }
    }
}
