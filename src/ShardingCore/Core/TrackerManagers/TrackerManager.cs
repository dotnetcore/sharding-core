using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.TrackerManagers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/23 22:37:59
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class TrackerManager<TShardingDbContext>: ITrackerManager<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ISet<Type> _dbContextModels = new HashSet<Type>();

        public bool AddDbContextModel(Type entityType)
        {
            return _dbContextModels.Add(entityType);
        }

        public bool EntityUseTrack(Type entityType)
        {
            return _dbContextModels.Contains(entityType);
        }

        public bool IsDbContextModel(Type entityType)
        {
            return _dbContextModels.Contains(entityType);
        }
    }
}
