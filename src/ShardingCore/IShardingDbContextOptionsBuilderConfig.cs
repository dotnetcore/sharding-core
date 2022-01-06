//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Text;
//using Microsoft.EntityFrameworkCore;
//using ShardingCore.Sharding.Abstractions;

///*
//* @Author: xjm
//* @Description:
//* @Date: 2021/8/20 11:34:55
//* @Ver: 1.0
//* @Email: 326308290@qq.com
//*/
//namespace ShardingCore
//{
//    /// <summary>
//    /// 分片db context配置构造配置
//    /// </summary>
//    /// <typeparam name="TShardingDbContext"></typeparam>
//    public interface IShardingDbContextOptionsBuilderConfig<TShardingDbContext>  where TShardingDbContext:DbContext,IShardingDbContext
//    {
//        /// <summary>
//        /// 如何根据connectionString 配置 DbContextOptionsBuilder
//        /// </summary>
//        /// <param name="connectionString"></param>
//        /// <param name="dbContextOptionsBuilder"></param>
//        /// <returns></returns>
//        DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString, DbContextOptionsBuilder dbContextOptionsBuilder);
//        /// <summary>
//        /// 如何根据dbConnection 配置DbContextOptionsBuilder
//        /// </summary>
//        /// <param name="dbConnection"></param>
//        /// <param name="dbContextOptionsBuilder"></param>
//        /// <returns></returns>
//        DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection, DbContextOptionsBuilder dbContextOptionsBuilder);
//    }
//}
