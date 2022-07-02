using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.TableCreator
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 21 December 2020 11:22:08
    * @Email: 326308290@qq.com
    */
    public interface IShardingTableCreator
    {
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tail"></param>
        /// <typeparam name="T"></typeparam>
        void CreateTable<T>(string dataSourceName, string tail) where T : class;
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        /// <exception cref="ShardingCreateException"></exception>
        void CreateTable(string dataSourceName, Type shardingEntityType, string tail);
    }
}