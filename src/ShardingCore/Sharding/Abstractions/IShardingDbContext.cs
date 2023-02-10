using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using System;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 21:47:11
    * @Email: 326308290@qq.com
    */
    public interface IShardingDbContext
    {
        /// <summary>
        /// 获取分片执行者
        /// </summary>
        /// <returns></returns>
        IShardingDbContextExecutor GetShardingExecutor();


    }
}