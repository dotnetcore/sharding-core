using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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
        Type ShardingDbContextType { get; }
        /// <summary>
        /// 真实的DbContext 类型
        /// </summary>
        Type ActualDbContextType {  get;}
        /// <summary>
        /// 创建DbContext
        /// </summary>
        /// <param name="track">true表示创建的dbcontext挂在当前的shardingdbcontext下无需管理生命周期，false需要手动释放，true not care dbcontext life, false need call dispose()</param>
        /// <param name="tail"></param>
        /// <returns></returns>
        DbContext GetDbContext(bool track,string tail);
        /// <summary>
        /// 根据实体创建db context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbContext CreateGenericDbContext<T>(T entity) where T : class;


        bool TryOpen();

    }

    public interface IShardingTableDbContext<T> : IShardingDbContext where T : DbContext, IShardingTableDbContext
    {

    }
}