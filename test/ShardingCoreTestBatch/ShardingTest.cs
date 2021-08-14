// using System;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading.Tasks;
// using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
// using ShardingCore.DbContexts.VirtualDbContexts;
// using ShardingCore.Extensions;
// using ShardingCoreTestBatch.Domain.Entities;
// using Xunit;
//
// namespace ShardingCoreTestBatch
// {
// /*
// * @Author: xjm
// * @Description:
// * @Date: Friday, 15 January 2021 17:22:10
// * @Email: 326308290@qq.com
// */
//     public class ShardingTest
//     {
//         private readonly IVirtualDbContext _virtualDbContext;
//         private readonly IRoutingRuleEngineFactory _routingRuleEngineFactory;
//
//         public ShardingTest(IVirtualDbContext virtualDbContext,IRoutingRuleEngineFactory routingRuleEngineFactory)
//         {
//             _virtualDbContext = virtualDbContext;
//             _routingRuleEngineFactory = routingRuleEngineFactory;
//         }
//
//         //[Fact]
//         //public async Task Route_TEST()
//         //{
//         //    var queryable1 = _virtualDbContext.Set<SysUserMod>().Where(o=>o.Id=="339");
//         //    var routeResults1 = _routingRuleEngineFactory.Route(queryable1);
//         //    Assert.Equal(1,routeResults1.Count());
//         //    Assert.Equal(1,routeResults1.FirstOrDefault().ReplaceTables.Count());
//         //    Assert.Equal("0",routeResults1.FirstOrDefault().ReplaceTables.FirstOrDefault().Tail);
//         //    Assert.Equal(nameof(SysUserMod),routeResults1.FirstOrDefault().ReplaceTables.FirstOrDefault().OriginalName);
//         //    var ids = new[] {"339", "124","142"};
//         //    var queryable2= _virtualDbContext.Set<SysUserMod>().Where(o=>ids.Contains(o.Id));
//         //    var routeResult2s = _routingRuleEngineFactory.Route(queryable2);
//         //    Assert.Equal(2,routeResult2s.Count());
//         //    Assert.Equal(1,routeResult2s.FirstOrDefault().ReplaceTables.Count());
//         //    Assert.Equal(2,routeResult2s.SelectMany(o=>o.ReplaceTables).Count());
//         //    Assert.Equal(true,routeResult2s.SelectMany(o=>o.ReplaceTables).All(o=>new[]{"0","1"}.Contains(o.Tail)));
//         //}
//         [Fact]
//         public async Task ToList_All_Test()
//         {
//             var startNew = Stopwatch.StartNew(); startNew.Start();
//              var mods = await _virtualDbContext.Set<SysUserMod>().Where(o=>o.Age== 7601935).ToShardingListAsync();
//              startNew.Stop();
//              var x = startNew.ElapsedMilliseconds;
//             Console.WriteLine(mods.Count);
//         }
//
//     }
// }