using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.TrackerManagers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/23 22:34:16
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface ITrackerManager<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        bool AddDbContextModel(Type entityType);
        bool EntityUseTrack(Type entityType);
    }
}
