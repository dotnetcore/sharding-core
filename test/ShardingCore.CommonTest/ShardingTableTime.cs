using System.Collections.ObjectModel;
using System.Reflection;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Helpers;
using ShardingCore.Utils;
using Xunit;

namespace ShardingCore.CommonTest
{

    
    public class ShardingTableTime
    {
        private readonly EntityMetadata _testEntityMetadata;
        private readonly List<string> _allTables;
        public ShardingTableTime()
        { 
            var entityMetadata = new EntityMetadata(typeof(TestTimeEntity));
            var entityMetadataTableBuilder = EntityMetadataTableBuilder<TestTimeEntity>.CreateEntityMetadataTableBuilder(entityMetadata);
            entityMetadataTableBuilder.ShardingProperty(o => o.Time);
            entityMetadata.CheckShardingTableMetadata();
            _testEntityMetadata = entityMetadata;
            var dateTime = new DateTime(2022,1,1);
            //[20220101....20220120]
            _allTables  = Enumerable.Range(0,20).Select(o=>dateTime.AddDays(o).ToString("yyyyMMdd")).ToList();
        }
        public static Func<string, bool> GetRouteFilter(object shardingValue, ShardingOperatorEnum shardingOperator,
            string propertyName)
        {
            if (propertyName != nameof(TestTimeEntity.Time))
            {
                throw new Exception($"{nameof(propertyName)}:[{propertyName}] error");
            }

            var shardingKey = (DateTime)shardingValue;
            var t =$"{shardingKey:yyyyMMdd}";
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    var shardingKeyDate = shardingKey.Date;
                    //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                    if (shardingKeyDate == shardingKey)
                        return tail =>String.Compare(tail, t, StringComparison.Ordinal) < 0;
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail =>String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
                    return tail => true;
                }
            }
        }
        
        private void TestId(IQueryable<TestTimeEntity> queryable, string[] tables)
        {
            var routePredicateExpression = ShardingUtil.GetRouteParseExpression(queryable,_testEntityMetadata,GetRouteFilter,true);
            Assert.NotNull(routePredicateExpression);
            var routePredicate = routePredicateExpression.GetRoutePredicate();
        
            var list = _allTables.Where(routePredicate).ToList();
            Assert.NotNull(list);
            Assert.Equal(tables.Length,list.Count);
            foreach (var table in tables)
            {
                Assert.True(list.Any(o=>o==table));
            }
        }

        private void TestFor(List<IQueryable<TestTimeEntity>> queryables, string[] tables)
        {
            foreach (var queryable in queryables)
            {
                TestId(queryable, tables);
            }
        }
        [Fact]
        public void TestSingleTable()
        {
            var queryTime = new DateTime(2022, 1, 2);
            var queryTime1 = queryTime.AddHours(1);
            var queryTime2 = new DateTime(2022, 1, 3);
            var queryTime3 = new DateTime(2022, 1, 3).AddSeconds(-1);
            var table = queryTime.ToString("yyyyMMdd");
            var tables = new []{table};
            var id = "1";
            var times = new []{queryTime};
            var times1 = new List<DateTime>(){queryTime};
            var times2 = new []{queryTime,queryTime2};
            var obj1 = new {time=new DateTime(2022, 1, 2)};
            var queryables=new List<IQueryable<TestTimeEntity>>()
            {
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time== new DateTime(2022, 1, 2)),
                new List<TestTimeEntity>().AsQueryable().CheckBetween((DateTime?)queryTime,(DateTime?)queryTime3,o=>o.Time),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time==times2[0]),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=times2[0]&&o.Time<times2[1]),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=queryTime&&o.Time<queryTime2),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time==queryTime),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=queryTime&&o.Time<queryTime2),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=queryTime&&o.Time<queryTime3),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=queryTime&&o.Time<=queryTime3),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time==queryTime1),
                new List<TestTimeEntity>().AsQueryable().Where(o=>queryTime==o.Time),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time.Equals(queryTime)),
                new List<TestTimeEntity>().AsQueryable().Where(o=>queryTime.Equals(o.Time)),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time==obj1.time),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time.Equals(obj1.time)),
                new List<TestTimeEntity>().AsQueryable().Where(o=>obj1.time.Equals(o.Time)),
                new List<TestTimeEntity>().AsQueryable().Where(o=>times.Contains(o.Time)),
                new List<TestTimeEntity>().AsQueryable().Where(o=>new []{queryTime}.Contains(o.Time)),
                new List<TestTimeEntity>().AsQueryable().Where(o=>times1.Contains(o.Time)),
                new List<TestTimeEntity>().AsQueryable().Where(o=> new List<DateTime>(){queryTime}.Contains(o.Time))
            };
            TestFor(queryables,tables);
        }
        //[20220101....20220120]
        [Fact]
        public void TestMultiTable()
        {
            var tables = new []{"20220101","20220102","20220119","20220120"};
            var begin = new DateTime(2022,01,03);
            var begin1 = new DateTime(2022,01,02);
            var end1 = new DateTime(2022,01,19);
            var begin2 = new DateTime(2022,01,01);
            var end2 = new DateTime(2022,01,20);
            var queryables=new List<IQueryable<TestTimeEntity>>()
            {
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time<begin||o.Time>=end1),
                new List<TestTimeEntity>().AsQueryable().Where(o=>begin>o.Time||end1<=o.Time),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time<=begin1||o.Time>=end1),
                new List<TestTimeEntity>().AsQueryable().Where(o=>begin1>=o.Time||o.Time>=end1),
                new List<TestTimeEntity>().AsQueryable().Where(o=>begin1>=o.Time||end1<=o.Time),
                new List<TestTimeEntity>().AsQueryable().Where(o=>(o.Time>=begin2&&o.Time<=begin1)||(o.Time>=end1&&o.Time<=end2)),
            };
            TestFor(queryables,tables);
        }
        [Fact]
        public void TestMultiTable1()
        {
            var tables = new []{"20220105","20220106","20220107"};
            var time1 = new DateTime(2022,01,05);
            var time2 = new DateTime(2022,01,07);
            var time3 = new DateTime(2022,01,08);
            var time4 = new DateTime(2022,01,06);
            var dateTimes = new []{time1,time2,time4};
            var queryables=new List<IQueryable<TestTimeEntity>>()
            {
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=time1&&o.Time<=time2),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time<=time2&&o.Time>=time1),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=time1.AddHours(1)&&o.Time<=time2),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=time1&&o.Time<time2.AddHours(1)),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=time1&&o.Time<time3),
                new List<TestTimeEntity>().AsQueryable().Where(o=>o.Time>=time1&&o.Time<=time3.AddHours(-1)),
                new List<TestTimeEntity>().AsQueryable().Where(o=>dateTimes.Contains(o.Time)),
            };
            TestFor(queryables,tables);
        }
    
    }
    public class TestTimeEntity
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
    }
}
