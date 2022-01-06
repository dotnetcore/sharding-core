//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.EntityFrameworkCore;
//using ShardingCore.Sharding.Abstractions;
//using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

//namespace ShardingCore.Sharding.ReadWriteConfigurations
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: 2021/9/7 11:06:40
//    * @Ver: 1.0
//    * @Email: 326308290@qq.com
//    */

//    public class ReadWriteOptions<TShardingDbContext> : IReadWriteOptions<TShardingDbContext>
//        where TShardingDbContext : DbContext, IShardingDbContext
//    {
//        public ReadWriteOptions(int readWritePriority, bool readWriteSupport, ReadStrategyEnum readStrategy, ReadConnStringGetStrategyEnum readConnStringGetStrategy)
//        {
//            ReadWritePriority = readWritePriority;
//            ReadWriteSupport = readWriteSupport;
//            ReadStrategy = readStrategy;
//            ReadConnStringGetStrategy = readConnStringGetStrategy;
//        }
//        public Type ShardingDbContextType => typeof(TShardingDbContext);
//        /// <summary>
//        /// 默认读写配置优先级
//        /// </summary>
//        public int ReadWritePriority { get;  }
//        /// <summary>
//        /// 默认是否开启读写分离
//        /// </summary>
//        public bool ReadWriteSupport { get; }

//        public ReadStrategyEnum ReadStrategy { get; }
//        public ReadConnStringGetStrategyEnum ReadConnStringGetStrategy { get; }
//    }
//}
