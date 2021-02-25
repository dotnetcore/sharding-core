using System;
using System.Collections.Generic;
using ShardingCore.Core.Internal;

namespace ShardingCore.Core.PhysicDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 13:05:28
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 物理表接口
    /// </summary>
    public interface IPhysicDataSource
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        string GetConnectionString();

        /// <summary>
        /// 数据源类型
        /// </summary>
        DataSourceEnum GetDataSourceType();


        /// <summary>
        /// 添加分库实体
        /// </summary>
        /// <param name="entityBaseType"></param>
        void AddEntity(ShardingEntityBaseType entityBaseType);

        /// <summary>
        /// 添加分库实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddEntity<T>() where T : class, IShardingDataSource;

        /// <summary>
        /// 添加分库实体
        /// </summary>
        /// <param name="shardingEntity"></param>
        void AddEntity(Type shardingEntity);
        /// <summary>
        /// 是否有对应的实体
        /// </summary>
        /// <param name="shardingEntity"></param>
        /// <returns></returns>
        bool HasEntity(Type shardingEntity);
        /// <summary>
        /// 是否有对应的实体
        /// </summary>
        /// <returns></returns>
        bool HasEntity<T>() where T : class, IShardingDataSource;
    }
}