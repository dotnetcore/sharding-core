using System.Collections.ObjectModel;
using System.Reflection;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Helpers;
using ShardingCore.Utils;
using Xunit;

namespace ShardingCore.CommonTest
{
    public class ShardingDataSource
    {
        private readonly EntityMetadata _testEntityMetadata;
        private readonly List<string> _allDataSources;
        public ShardingDataSource()
        { 
            var entityMetadata = new EntityMetadata(typeof(TestEntity), nameof(TestEntity), typeof(ShardingDataSource),
                new ReadOnlyCollection<PropertyInfo>(typeof(TestEntity).GetProperties().ToList()), null);
            var entityMetadataDataSourceBuilder = EntityMetadataDataSourceBuilder<TestEntity>.CreateEntityMetadataDataSourceBuilder(entityMetadata);
            entityMetadataDataSourceBuilder.ShardingProperty(o => o.Id);
            entityMetadata.CheckShardingDataSourceMetadata();
            _testEntityMetadata = entityMetadata;
            _allDataSources  = Enumerable.Range(0,10).Select(o=>o.ToString()).ToList();
        }

        public static Func<string, bool> GetRouteFilter(object shardingValue, ShardingOperatorEnum shardingOperator,
            string propertyName)
        {
            if (propertyName != nameof(TestEntity.Id))
            {
                throw new Exception($"{nameof(propertyName)}:[{propertyName}] error");
            }

            var stringHashCode = ShardingCoreHelper.GetStringHashCode(shardingValue.ToString());
            var dataSourceName = (stringHashCode%10).ToString();
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return t => t == dataSourceName;
                default:
                {
                    return t => true;
                }
            }
        }

        private void TestId1(IQueryable<TestEntity> queryable)
        {
            var routePredicateExpression = ShardingUtil.GetRouteParseExpression(queryable,_testEntityMetadata,GetRouteFilter,false);
            Assert.NotNull(routePredicateExpression);
            var routePredicate = routePredicateExpression.GetRoutePredicate();
        
            var list = _allDataSources.Where(routePredicate).ToList();
            Assert.NotNull(list);
            Assert.Equal(1,list.Count);
            var stringHashCode = ShardingCoreHelper.GetStringHashCode("1");
            var dataSourceName = (stringHashCode%10).ToString();
            Assert.Equal(dataSourceName,list[0]);
        }
        [Fact]
        public void TestSingleDataSource()
        {
            var constantQueryable1 = new List<TestEntity>().AsQueryable().Where(o=>o.Id=="1");
            TestId1(constantQueryable1);
            var constantQueryable2 = new List<TestEntity>().AsQueryable().Where(o=>"1"==o.Id);
            TestId1(constantQueryable2);
            var id = "1";
            var constantQueryable3 = new List<TestEntity>().AsQueryable().Where(o=>o.Id==id);
            TestId1(constantQueryable3);
            var constantQueryable4 = new List<TestEntity>().AsQueryable().Where(o=>id==o.Id);
            TestId1(constantQueryable4);
            var constantQueryable5 = new List<TestEntity>().AsQueryable().Where(o=>o.Id.Equals("1"));
            TestId1(constantQueryable5);
            var constantQueryable6 = new List<TestEntity>().AsQueryable().Where(o=>"1".Equals(o.Id));
            TestId1(constantQueryable6);
            var constantQueryable7 = new List<TestEntity>().AsQueryable().Where(o=>o.Id.Equals(id));
            TestId1(constantQueryable7);
            var constantQueryable8 = new List<TestEntity>().AsQueryable().Where(o=>o.Id.Equals(id));
            TestId1(constantQueryable8);
            var ids = new []{"1"};
            var constantQueryable9 = new List<TestEntity>().AsQueryable().Where(o=>ids.Contains(o.Id));
            TestId1(constantQueryable9);
            var constantQueryable10 = new List<TestEntity>().AsQueryable().Where(o=>new []{"1"}.Contains(o.Id));
            TestId1(constantQueryable10);
            var ids1 = new List<string>(){"1"};
            var constantQueryable11 = new List<TestEntity>().AsQueryable().Where(o=>ids1.Contains(o.Id));
            TestId1(constantQueryable11);
            var constantQueryable12 = new List<TestEntity>().AsQueryable().Where(o=>new List<string>(){"1"}.Contains(o.Id));
            TestId1(constantQueryable12);
            var obj1 = new {Id="1"};
            var constantQueryable13 = new List<TestEntity>().AsQueryable().Where(o=>o.Id==obj1.Id);
            TestId1(constantQueryable13);
            var constantQueryable14 = new List<TestEntity>().AsQueryable().Where(o=>obj1.Id==o.Id);
            TestId1(constantQueryable14);
            var constantQueryable15 = new List<TestEntity>().AsQueryable().Where(o=>o.Id.Equals(obj1.Id));
            TestId1(constantQueryable15);
            var constantQueryable16 = new List<TestEntity>().AsQueryable().Where(o=>obj1.Id.Equals(o.Id));
            TestId1(constantQueryable16);
            var constantQueryable17 = new List<TestEntity>().AsQueryable().Where(o=>new []{obj1.Id}.Contains(o.Id));
            TestId1(constantQueryable17);
        }

        [Fact]
        public void TestTwoDataSource()
        {
            
        }
    }

    public class TestEntity
    {
        public string Id { get; set; }
    }
}
