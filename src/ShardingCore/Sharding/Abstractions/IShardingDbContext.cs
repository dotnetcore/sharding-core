using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.Abstractions;

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
        /// ��ʵ��DbContext ����
        /// </summary>
        Type ActualDbContextType {  get;}
        /// <summary>
        /// create DbContext
        /// </summary>
        /// <param name="track">true not care dbcontext life, false need call dispose()</param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        DbContext GetDbContext(bool track,IRouteTail routeTail);
        /// <summary>
        /// ����ʵ�崴��db context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbContext CreateGenericDbContext<T>(T entity) where T : class;
        

    }

    public interface IShardingTableDbContext<T> : IShardingDbContext where T : DbContext, IShardingTableDbContext
    {

    }
}