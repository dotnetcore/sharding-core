using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/4 13:11:16
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingDbContextCreatorConfig
    {
        Type ShardingDbContextType { get; }
        Type ActualDbContextType { get; }

        DbContext Creator(ShardingDbContextOptions shardingDbContextOptions);

    }
}
