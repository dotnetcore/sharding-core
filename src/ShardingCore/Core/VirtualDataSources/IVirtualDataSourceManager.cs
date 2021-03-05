using System;
using System.Collections.Generic;

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
        /// <summary>
        /// 添加链接
        /// </summary>
        /// <param name="connectKey"></param>
        void AddShardingConnectKey(string connectKey);
        /// <summary>
        /// 获取所有的链接
        /// </summary>
        /// <returns></returns>
        ISet<string> GetAllShardingConnectKeys();


        /// <summary>
        /// 添加虚拟数据源应用启动时 add virtual table when app start
        /// </summary>
        /// <param name="virtualDataSource"></param>
        void AddVirtualDataSource(IVirtualDataSource virtualDataSource);
        /// <summary>
        /// 获取所有的虚拟连接 get all virtual table
        /// </summary>
        /// <returns></returns>
        List<IVirtualDataSource> GetAllVirtualDataSources();

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
        /// <summary>
        /// 获取虚拟表 get virtual table by sharding entity type
        /// </summary>
        /// <returns></returns>     
        IVirtualDataSource<T> GetVirtualDataSource<T>() where T:class,IShardingDataSource;

        string GetDefaultConnectKey();
        bool IsShardingDataSource();
        bool HasVirtualShardingDataSourceRoute(Type shardingEntityType);

        List<string> GetEntityTypeLinkedConnectKeys(Type shardingEntityType);

    }
}