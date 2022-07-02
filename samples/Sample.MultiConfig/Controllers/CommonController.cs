// using System.Data.Common;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using ShardingCore.Core;
// using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
// using ShardingCore.DynamicDataSources;
// using ShardingCore.Sharding.ReadWriteConfigurations;
// using ShardingCore.Sharding.ShardingComparision.Abstractions;
// using ShardingCore.TableExists.Abstractions;
//
// namespace Sample.MultiConfig.Controllers
// {
//     [ApiController]
//     [Route("[controller]/[action]")]
//     public class CommonController : ControllerBase
//     {
//         private readonly IVirtualDataSourceManager<MultiConfigDbContext> _virtualDataSourceManager;
//
//         public CommonController(IVirtualDataSourceManager<MultiConfigDbContext> virtualDataSourceManager)
//         {
//             _virtualDataSourceManager = virtualDataSourceManager;
//         }
//         public async Task<IActionResult> DynamicAdd(string configId)
//         {
//             var flag = DynamicDataSourceHelper.DynamicAppendVirtualDataSource(new MySqlConfigurationParam(configId));
//             return Ok(flag?"成功":"失败");
//         }
//     }
//
//     public class MySqlConfigurationParam : AbstractVirtualDataSourceConfigurationParams<MultiConfigDbContext>
//     {
//         public override string ConfigId { get; }
//         public override int Priority { get; }
//         public override string DefaultDataSourceName { get; }
//         public override string DefaultConnectionString { get; }
//         public override IDictionary<string, ReadNode[]> ReadWriteNodeSeparationConfigs { get; }
//         public override ReadStrategyEnum? ReadStrategy { get; }
//         public override bool? ReadWriteDefaultEnable { get; }
//         public override int? ReadWriteDefaultPriority { get; }
//         public override ReadConnStringGetStrategyEnum? ReadConnStringGetStrategy { get; }
//
//         public MySqlConfigurationParam(string configId)
//         {
//             ConfigId = configId;
//             DefaultDataSourceName = "ds0";
//             DefaultConnectionString = $"server=127.0.0.1;port=3306;database=MultiConfigSharding{configId};userid=root;password=L6yBtV6qNENrwBy7;";
//         }
//
//         public override DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString,
//             DbContextOptionsBuilder dbContextOptionsBuilder)
//         {
//             dbContextOptionsBuilder.UseMySql(connectionString,new MySqlServerVersion(new Version()));
//             return dbContextOptionsBuilder;
//         }
//
//         public override DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection,
//             DbContextOptionsBuilder dbContextOptionsBuilder)
//         {
//             dbContextOptionsBuilder.UseMySql(dbConnection, new MySqlServerVersion(new Version()));
//             return dbContextOptionsBuilder;
//         }
//
//         public override void UseShellDbContextOptionBuilder(DbContextOptionsBuilder dbContextOptionsBuilder)
//         {
//             
//         }
//
//         public override void UseExecutorDbContextOptionBuilder(DbContextOptionsBuilder dbContextOptionsBuilder)
//         {
//             
//         }
//     }
// }
