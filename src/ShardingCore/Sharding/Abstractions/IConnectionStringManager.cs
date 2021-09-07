using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 10:29:38
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IConnectionStringManager
    {
        Type ShardingDbContextType { get; }
        string GetConnectionString(IShardingDbContext shardingDbContext);
    }
}
