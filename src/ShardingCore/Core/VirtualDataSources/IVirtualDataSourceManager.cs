using System;
using System.Collections.Generic;
using ShardingCore.Core.PhysicDataSources;

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
        /// 获取所有的虚拟连接 get all virtual table
        /// </summary>
        /// <returns></returns>
        List<IVirtualDataSource> GetAllVirtualDataSources();
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

        
        /// <summary>
        /// 添加虚拟数据源应用启动时 add virtual table when app start
        /// </summary>
        /// <param name="virtualDataSource"></param>
        void AddVirtualDataSource(IVirtualDataSource virtualDataSource);
        /// <summary>
        /// 添加物理表 add physic table
        /// </summary>
        /// <param name="virtualDataSource"></param>
        /// <param name="physicDataSource"></param>
        void AddPhysicDataSource(IVirtualDataSource virtualDataSource, IPhysicDataSource physicDataSource);
        /// <summary>
        /// 添加物理表 add physic table
        /// </summary>
        /// <param name="shardingEntityType"></param>
        /// <param name="physicDataSource"></param>
        void AddPhysicDataSource(Type shardingEntityType, IPhysicDataSource physicDataSource);
    }
}