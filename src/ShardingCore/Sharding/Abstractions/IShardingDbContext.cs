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
        /// <summary>
        /// 真实的DbContext 类型
        /// </summary>
       Type ActualDbContextType {  get;}
        /// <summary>
        /// 创建DbContext
        /// </summary>
        /// <param name="track"></param>
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


    }
}