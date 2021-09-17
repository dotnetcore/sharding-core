using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 06 February 2021 14:24:01
* @Email: 326308290@qq.com
*/
    public interface IVirtualDataSourceManager
    {
        IPhysicDataSource GetDataSource(string dsName);
        /// <summary>
        /// 添加链接
        /// </summary>
        /// <param name="physicDataSource"></param>
        void AddPhysicDataSource(IPhysicDataSource physicDataSource);
        /// <summary>
        /// 获取默认的数据源
        /// </summary>
        /// <param name="shardingDbContextType"></param>
        /// <returns></returns>
        IPhysicDataSource GetDefaultDataSource(Type shardingDbContextType);
        IVirtualDataSource GetVirtualDataSource(Type shardingDbContextType, Type entityType);
        /// <summary>
        /// 获取虚拟表 get virtual table by sharding entity type
        /// </summary>
        /// <returns></returns>     
        IVirtualDataSource<T> GetVirtualDataSource<T>() where T : class, IShardingDataSource;
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="shardingDbContextType"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        List<IPhysicDataSource> GetDefaultDataSources(Type shardingDbContextType,Type entityType);
        


        /// <summary>
        /// 添加虚拟数据源应用启动时 add virtual table when app start
        /// </summary>
        /// <param name="virtualDataSource"></param>
        void AddVirtualDataSource(IVirtualDataSource virtualDataSource);

        /// <summary>
        /// 添加链接对应的对象
        /// </summary>
        /// <param name="connectKey"></param>
        /// <param name="entityType"></param>
        void AddConnectEntities(string connectKey,Type entityType);


        /// <summary>
        /// 获取虚拟表 get virtual table by sharding entity type
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <returns></returns>
        IVirtualDataSource GetVirtualDataSource(Type shardingEntityType);  


        List<string> GetEntityTypeLinkedConnectKeys(Type shardingEntityType);

    }
}