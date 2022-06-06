// using System.Collections.ObjectModel;
// using System.Reflection;
// using ShardingCore.Core.EntityMetadatas;
// using ShardingCore.Core.VirtualRoutes;
// using ShardingCore.Helpers;
// using ShardingCore.Utils;
// using Xunit;
//
// namespace ShardingCore.CommonTest
// {
//     /// <summary>
//     /// 测试按年分库,按月分表
//     /// </summary>
//     public class ShardingTimeAll
//     {
//         private readonly EntityMetadata _testEntityMetadata;
//         private readonly List<string> _allTables;
//         private readonly List<string> _allDataSources;
//         public ShardingTimeAll()
//         { 
//             var entityMetadata = new EntityMetadata(typeof(TestTimeShardingEntity), nameof(TestTimeShardingEntity), typeof(ShardingDataSourceMod),
//                 new ReadOnlyCollection<PropertyInfo>(typeof(TestTimeShardingEntity).GetProperties().ToList()), null);
//             //分库
//             var entityMetadataDataSourceBuilder = EntityMetadataDataSourceBuilder<TestTimeShardingEntity>.CreateEntityMetadataDataSourceBuilder(entityMetadata);
//             entityMetadataDataSourceBuilder.ShardingProperty(o => o.Time);
//             entityMetadata.CheckShardingDataSourceMetadata();
//             //分表
//             var entityMetadataTableBuilder = EntityMetadataTableBuilder<TestTimeShardingEntity>.CreateEntityMetadataTableBuilder(entityMetadata);
//             entityMetadataTableBuilder.ShardingProperty(o => o.Time);
//             entityMetadata.CheckShardingTableMetadata();
//             _testEntityMetadata = entityMetadata;
//             //[01....12]
//             _allTables  = Enumerable.Range(1,13).Select(o=>o.ToString().PadLeft(2,'0')).ToList();
//             _allDataSources = new[] { "2020", "2021", "2022" }.ToList();
//         }
//         
//         public static Func<string, bool> GetDataSourceRouteFilter(object shardingValue, ShardingOperatorEnum shardingOperator,
//             string propertyName)
//         {
//             if (propertyName != nameof(TestTimeEntity.Time))
//             {
//                 throw new Exception($"{nameof(propertyName)}:[{propertyName}] error");
//             }
//
//             var shardingKey = (DateTime)shardingValue;
//             var t = $"{shardingKey:yyyy}";
//             switch (shardingOperator)
//             {
//                 case ShardingOperatorEnum.GreaterThan:
//                 case ShardingOperatorEnum.GreaterThanOrEqual:
//                     return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
//                 case ShardingOperatorEnum.LessThan:
//                 {
//                     var currentYear =new DateTime(shardingKey.Year,1,1);
//                     //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
//                     if (currentYear == shardingKey)
//                         return tail => String.Compare(tail, t, StringComparison.Ordinal) < 0;
//                     return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
//                 }
//                 case ShardingOperatorEnum.LessThanOrEqual:
//                     return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
//                 case ShardingOperatorEnum.Equal: return tail => tail == t;
//                 default:
//                 {
//                     return tail => true;
//                 }
//             }
//         }
//         public static Func<string, bool> GetTableRouteFilter(object shardingValue, ShardingOperatorEnum shardingOperator,
//             string propertyName)
//         {
//             if (propertyName != nameof(TestTimeEntity.Time))
//             {
//                 throw new Exception($"{nameof(propertyName)}:[{propertyName}] error");
//             }
//
//             var shardingKey = (DateTime)shardingValue;
//             var t = $"{shardingKey:MM}";
//             switch (shardingOperator)
//             {
//                 case ShardingOperatorEnum.GreaterThan:
//                 case ShardingOperatorEnum.GreaterThanOrEqual:
//                     return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
//                 case ShardingOperatorEnum.LessThan:
//                 {
//                     var currentMonth = ShardingCoreHelper.GetCurrentMonthFirstDay(shardingKey);
//                     //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
//                     if (currentMonth == shardingKey)
//                         return tail => String.Compare(tail, t, StringComparison.Ordinal) < 0;
//                     return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
//                 }
//                 case ShardingOperatorEnum.LessThanOrEqual:
//                     return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
//                 case ShardingOperatorEnum.Equal: return tail => tail == t;
//                 default:
//                 {
//                     return tail => true;
//                 }
//             }
//         }
//         
//         
//         private void TestDataSource(IQueryable<TestTimeShardingEntity> queryable, string[] dataSources)
//         {
//             var routePredicateExpression = ShardingUtil.GetRouteParseExpression(queryable,_testEntityMetadata,GetDataSourceRouteFilter,false);
//             Assert.NotNull(routePredicateExpression);
//             var routePredicate = routePredicateExpression.GetRoutePredicate();
//         
//             var list = _allDataSources.Where(routePredicate).ToList();
//             Assert.NotNull(list);
//             Assert.Equal(dataSources.Length,list.Count);
//             foreach (var table in dataSources)
//             {
//                 Assert.True(list.Any(o=>o==table));
//             }
//         }
//         private void TestTable(IQueryable<TestTimeShardingEntity> queryable, string[] tables)
//         {
//             var routePredicateExpression = ShardingUtil.GetRouteParseExpression(queryable,_testEntityMetadata,GetTableRouteFilter,true);
//             Assert.NotNull(routePredicateExpression);
//             var routePredicate = routePredicateExpression.GetRoutePredicate();
//         
//             var list = _allTables.Where(routePredicate).ToList();
//             Assert.NotNull(list);
//             Assert.Equal(tables.Length,list.Count);
//             foreach (var table in tables)
//             {
//                 Assert.True(list.Any(o=>o==table));
//             }
//         }
//
//         private void TestForDataSource(List<IQueryable<TestTimeShardingEntity>> queryables, string[] dataSources)
//         {
//             foreach (var queryable in queryables)
//             {
//                 TestDataSource(queryable, dataSources);
//             }
//         }
//         private void TestForTable(List<IQueryable<TestTimeShardingEntity>> queryables, string[] tables)
//         {
//             foreach (var queryable in queryables)
//             {
//                 TestTable(queryable, tables);
//             }
//         }
//         [Fact]
//         public void TestSingleDataSource()
//         {
//             var dataSources = new []{"2021"};
//             var tables = new []{"01","02","03","04","05","06","07","08"};
//             var queryTime = new DateTime(2021, 1, 2);
//             var queryTime1 = queryTime.AddMonths(7);
//             var queryables=new List<IQueryable<TestTimeShardingEntity>>()
//             {
//                 new List<TestTimeShardingEntity>().AsQueryable().Where(o=>o.Time>=queryTime&&o.Time<=queryTime1),
//                 new List<TestTimeShardingEntity>().AsQueryable().Where(o=>o.Time<=queryTime1&&o.Time>=queryTime)
//             };
//             TestForDataSource(queryables,dataSources);
//             TestForTable(queryables,tables);
//         }
//         [Fact]
//         public void TestMultiDataSource()
//         {
//             var dataSources = new []{"2021","2020"};
//             var tables = new []{"01","02","03","04","05","06","07","08"};
//             var queryTime = new DateTime(2021, 7, 2);
//             var queryTime1 =  new DateTime(2022, 3, 2);
//             var queryables=new List<IQueryable<TestTimeShardingEntity>>()
//             {
//                 new List<TestTimeShardingEntity>().AsQueryable().Where(o=>o.Time>=queryTime&&o.Time<=queryTime1),
//                 new List<TestTimeShardingEntity>().AsQueryable().Where(o=>o.Time<=queryTime1&&o.Time>=queryTime)
//             };
//             TestForDataSource(queryables,dataSources);
//             TestForTable(queryables,tables);
//         }
//     }
//     public class TestTimeShardingEntity
//     {
//         public string Id { get; set; }
//         public DateTime Time { get; set; }
//     }
// }