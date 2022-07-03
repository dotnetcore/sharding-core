// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Sample.SqlServer.DbContexts;
// using Sample.SqlServer.Domain.Entities;
// using ShardingCore.Core.EntityMetadatas;
// using ShardingCore.Core.PhysicTables;
// using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
// using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
// using ShardingCore.Core.VirtualDatabase.VirtualTables;
// using ShardingCore.Core.VirtualTables;
// using ShardingCore.Extensions;
// using ShardingCore.TableCreator;
//
// namespace Sample.SqlServer.Controllers
// {
//     [ApiController]
//     [Route("[controller]/[action]")]
//     public class CreateTableController : ControllerBase
//     {
//         private readonly IShardingTableCreator _tableCreator;
//         private readonly IVirtualDataSourceManager<DefaultShardingDbContext> _virtualDataSourceManager;
//         private readonly IVirtualTableManager<DefaultShardingDbContext> _virtualTableManager;
//         private readonly IEntityMetadataManager<DefaultShardingDbContext> _entityMetadataManager;
//
//         public CreateTableController(IShardingTableCreator<DefaultShardingDbContext> tableCreator,
//             IVirtualDataSourceManager<DefaultShardingDbContext> virtualDataSourceManager,
//             IVirtualTableManager<DefaultShardingDbContext> virtualTableManager,
//             IEntityMetadataManager<DefaultShardingDbContext> entityMetadataManager)
//         {
//             _tableCreator = tableCreator;
//             _virtualDataSourceManager = virtualDataSourceManager;
//             _virtualTableManager = virtualTableManager;
//             _entityMetadataManager = entityMetadataManager;
//         }
//         [HttpGet]
//         public IActionResult Get()
//         {
//             var isShardingTable = _entityMetadataManager.IsShardingTable<SysUserMod>();
//             if (isShardingTable)
//             {
//                 #region 完全可以用脚本实现这段代码
//                 var defaultDataSourceName = _virtualDataSourceManager.GetCurrentVirtualDataSource().DefaultDataSourceName;
//                 try
//                 {
//                     _tableCreator.CreateTable<SysUserMod>(defaultDataSourceName, "09");
//                 }
//                 catch (Exception e)
//                 {
//                     Console.WriteLine(e);
//                 } 
//                 #endregion
//                 //告诉系统SysUserMod 有一张09的表
//                 var virtualTable = _virtualTableManager.GetVirtualTable<SysUserMod>();
//                 _virtualTableManager.AddPhysicTable(virtualTable,new DefaultPhysicTable(virtualTable, "09"));
//             }
//
//             return BadRequest();
//         }
//     }
// }
