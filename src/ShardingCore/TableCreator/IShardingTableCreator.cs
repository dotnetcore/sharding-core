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
/// <summary>
/// 
/// </summary>
    public interface IShardingTableCreator
    {
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="tail"></param>
        /// <typeparam name="T"></typeparam>
        void CreateTable<TShardingDbContext,T>(string tail) where T : class, IShardingTable where TShardingDbContext:DbContext,IShardingDbContext;
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="shardingDbContextType"></param>
        /// <param name="shardingEntityType"></param>
        /// <param name="tail"></param>
        /// <exception cref="ShardingCreateException"></exception>
        void CreateTable(Type shardingDbContextType,Type shardingEntityType,string tail);
    }
}