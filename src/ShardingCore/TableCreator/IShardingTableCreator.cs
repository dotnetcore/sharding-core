using System;
using ShardingCore.Core;
using ShardingCore.Exceptions;

namespace ShardingCore.TableCreator
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 21 December 2020 11:22:08
* @Email: 326308290@qq.com
*/
/// <summary>
/// 
/// </summary>
    public interface IShardingTableCreator
    {
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="connectKey"></param>
        /// <param name="tail"></param>
        /// <typeparam name="T"></typeparam>
        void CreateTable<T>(string connectKey,string tail) where T : class, IShardingEntity;
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="connectKey"></param>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        /// <exception cref="ShardingCreateException"></exception>
        void CreateTable(string connectKey, Type shardingEntityType,string tail);
    }
}