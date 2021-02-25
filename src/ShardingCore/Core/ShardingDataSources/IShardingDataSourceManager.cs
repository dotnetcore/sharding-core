using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Core.ShardingDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 18 February 2021 17:12:22
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 分库数据源管理
    /// </summary>
    public interface IShardingDataSourceManager
    {
       /// <summary>
       /// 添加对象数据源
       /// </summary>
       /// <param name="connectKey"></param>
       /// <param name="connectionString"></param>
       /// <typeparam name="T"></typeparam>
       /// <typeparam name="TDbContext"></typeparam>
        void AddDataSource<T, TDbContext>(string connectKey,string connectionString) where T : IShardingDataSource
            where TDbContext : DbContext;
       
       /// <summary>
       /// 是否需要分数据库
       /// </summary>
       /// <returns></returns>
       bool IsShardingDataSource();

       public IEnumerable<ShardingDataSourceDbEntry> FilterDataSources(ISet<Type> queryEntities);
    }
}