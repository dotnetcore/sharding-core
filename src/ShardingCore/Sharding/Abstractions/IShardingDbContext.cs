using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.RouteTails.Abstractions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

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
        /// 当前sharding的db context type
        /// </summary>
        Type ShardingDbContextType { get; }
        /// <summary>
        /// 真实的db context type
        /// </summary>
        Type ActualDbContextType {  get;}
        /// <summary>
        /// create DbContext
        /// </summary>
        /// <param name="track">true not care db context life, false need call dispose()</param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        DbContext GetDbContext(bool track,IRouteTail routeTail);
        /// <summary>
        /// 创建通用的db context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbContext CreateGenericDbContext<T>(T entity) where T : class;
        /// <summary>
        /// 根据表达式创建db context
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>

        IEnumerable<DbContext> CreateExpressionDbContext<TEntity>(Expression<Func<TEntity, bool>> where)
            where TEntity : class;

        string GetConnectionString();


    }

    public interface IShardingDbContext<T> : IShardingDbContext where T : DbContext, IShardingTableDbContext
    {

    }
}