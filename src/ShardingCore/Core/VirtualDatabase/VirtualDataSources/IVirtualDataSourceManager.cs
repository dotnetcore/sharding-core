//using System;
//using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore;
//using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
//using ShardingCore.Core.VirtualRoutes;
//using ShardingCore.Sharding.Abstractions;

//namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: Saturday, 06 February 2021 14:24:01
//    * @Email: 326308290@qq.com
//    */
//    public interface IVirtualDataSourceManager
//    {
//        /// <summary>
//        /// 添加链接
//        /// </summary>
//        /// <param name="physicDataSource"></param>
//        bool AddPhysicDataSource(IPhysicDataSource physicDataSource);
//        IVirtualDataSource GetVirtualDataSource();
//        IPhysicDataSource GetDefaultDataSource();
//        string GetDefaultDataSourceName();
//        IPhysicDataSource GetPhysicDataSource(string dataSourceName);
//    }
//    public interface IVirtualDataSourceManager<TShardingDbContext> : IVirtualDataSourceManager where TShardingDbContext : DbContext, IShardingDbContext
//    {




//    }
//}