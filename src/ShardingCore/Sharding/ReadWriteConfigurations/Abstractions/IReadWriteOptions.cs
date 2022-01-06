//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.EntityFrameworkCore;
//using ShardingCore.Sharding.Abstractions;

//namespace ShardingCore.Sharding.ReadWriteConfigurations.Abstractions
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: 2021/9/7 11:13:52
//    * @Ver: 1.0
//    * @Email: 326308290@qq.com
//    */
//    public interface IReadWriteOptions<TShardingDbContext> 
//        where TShardingDbContext : DbContext, IShardingDbContext
//    {
//        /// <summary>
//        /// 默认读写配置优先级
//        /// </summary>
//        int ReadWritePriority { get; }
//        /// <summary>
//        /// 默认是否开启读写分离
//        /// </summary>
//        bool ReadWriteSupport { get; }
//        ReadStrategyEnum ReadStrategy { get; }
//        ReadConnStringGetStrategyEnum ReadConnStringGetStrategy { get; }
//    }
//}
