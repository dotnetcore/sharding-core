// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
// using ShardingCore.Sharding.Abstractions;
//
// namespace ShardingCore.DynamicDataSources
// {
//     public interface IDataSourceInitializer<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
//     {
//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="virtualDataSource"></param>
//         /// <param name="dataSourceName"></param>
//         void InitConfigure(IVirtualDataSource<TShardingDbContext> virtualDataSource, string dataSourceName);
//     }
// }
