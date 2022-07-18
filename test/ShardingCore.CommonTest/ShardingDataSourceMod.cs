using System.Collections.ObjectModel;
using System.Reflection;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Helpers;
using ShardingCore.Utils;
using Xunit;

namespace ShardingCore.CommonTest
{
    public class ShardingDataSourceMod
    {
        private readonly EntityMetadata _testEntityMetadata;
        private readonly List<string> _allDataSources;

        public ShardingDataSourceMod()
        {
            var entityMetadata = new EntityMetadata(typeof(TestEntity));
            var entityMetadataDataSourceBuilder =
                EntityMetadataDataSourceBuilder<TestEntity>.CreateEntityMetadataDataSourceBuilder(entityMetadata);
            entityMetadataDataSourceBuilder.ShardingProperty(o => o.Id);
            entityMetadata.CheckShardingDataSourceMetadata();
            _testEntityMetadata = entityMetadata;
            _allDataSources = Enumerable.Range(0, 10).Select(o => o.ToString()).ToList();
        }

        public static Func<string, bool> GetRouteFilter(object shardingValue, ShardingOperatorEnum shardingOperator,
            string propertyName)
        {
            if (propertyName != nameof(TestEntity.Id))
            {
                throw new Exception($"{nameof(propertyName)}:[{propertyName}] error");
            }

            var stringHashCode = ShardingCoreHelper.GetStringHashCode(shardingValue.ToString());
            var dataSourceName = (stringHashCode % 10).ToString();
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return t => t == dataSourceName;
                default:
                {
                    return t => true;
                }
            }
        }

        private void TestId(IQueryable<TestEntity> queryable, string[] dataSourceNames)
        {
            var routePredicateExpression =
                ShardingUtil.GetRouteParseExpression(queryable, _testEntityMetadata, GetRouteFilter, false);
            Assert.NotNull(routePredicateExpression);
            var routePredicate = routePredicateExpression.GetRoutePredicate();

            var list = _allDataSources.Where(routePredicate).ToList();
            Assert.NotNull(list);
            Assert.Equal(dataSourceNames.Length, list.Count);
            foreach (var dataSourceName in dataSourceNames)
            {
                Assert.True(list.Any(o => o == dataSourceName));
            }
        }

        private void TestFor(List<IQueryable<TestEntity>> queryables, string[] dataSourceNames)
        {
            foreach (var queryable in queryables)
            {
                TestId(queryable, dataSourceNames);
            }
        }

        [Fact]
        public void TestSingleDataSource()
        {
            var stringHashCode = ShardingCoreHelper.GetStringHashCode("1");
            var dataSourceName = (stringHashCode % 10).ToString();
            var dataSourceNames = new[] { dataSourceName };
            var dataSourceNames1 = Enumerable.Range(0, 10).Select(o => o.ToString()).ToArray();;
            var id = "1";
            var ids = new[] { "1" };
            var ids1 = new List<string>() { "1" };
            var obj1 = new { Id = "1" };
            var queryables = new List<IQueryable<TestEntity>>()
            {
                new List<TestEntity>().AsQueryable().Where(o => o.Id == "1"),
                new List<TestEntity>().AsQueryable().Where(o => o.Id == "1" && o.Id != "2"),
                new List<TestEntity>().AsQueryable().Where(o => "1" == o.Id),
                new List<TestEntity>().AsQueryable().Where(o => o.Id == id),
                new List<TestEntity>().AsQueryable().Where(o => id == o.Id),
                new List<TestEntity>().AsQueryable().Where(o => o.Id.Equals("1")),
                new List<TestEntity>().AsQueryable().Where(o => "1".Equals(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => o.Id.Equals(id)),
                new List<TestEntity>().AsQueryable().Where(o => o.Id.Equals(id)),
                new List<TestEntity>().AsQueryable().Where(o => ids.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => new[] { "1" }.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => ids1.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => new List<string>() { "1" }.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => o.Id == obj1.Id),
                new List<TestEntity>().AsQueryable().Where(o => obj1.Id == o.Id),
                new List<TestEntity>().AsQueryable().Where(o => o.Id.Equals(obj1.Id)),
                new List<TestEntity>().AsQueryable().Where(o => obj1.Id.Equals(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => new[] { obj1.Id }.Contains(o.Id))
            };
            TestFor(queryables, dataSourceNames);
            var queryables1 = new List<IQueryable<TestEntity>>()
            {
                new List<TestEntity>().AsQueryable().Where(o => o.Id.Contains("1")),
                new List<TestEntity>().AsQueryable().Where(o => o.Id.StartsWith("1")),
                new List<TestEntity>().AsQueryable().Where(o => o.Id.EndsWith("1"))
            };
            TestFor(queryables1, dataSourceNames1);
        }

        [Fact]
        public void TestTwoDataSource()
        {
            var dataSourceNames = new[]
            {
                (ShardingCoreHelper.GetStringHashCode("1") % 10).ToString(),
                (ShardingCoreHelper.GetStringHashCode("2") % 10).ToString()
            };
            var ids = new[] { "1", "2" };
            var ids1 = new List<string>() { "1", "2" };
            var obj1 = new { Id = "1" };
            var obj2 = new { Id = "2" };
            var queryables = new List<IQueryable<TestEntity>>()
            {
                new List<TestEntity>().AsQueryable().Where(o => o.Id == "1" || o.Id == "2"),
                new List<TestEntity>().AsQueryable().Where(o => (o.Id == "1" || o.Id == "2") && o.Id != "3"),
                new List<TestEntity>().AsQueryable().Where(o => (o.Id == "1" || o.Id == "2") && !o.Id.Equals("3")),
                new List<TestEntity>().AsQueryable().Where(o => "1" == o.Id || o.Id == "2"),
                new List<TestEntity>().AsQueryable().Where(o => "1" == o.Id || "2" == o.Id),
                new List<TestEntity>().AsQueryable().Where(o => o.Id.Equals("1") || o.Id == "2"),
                new List<TestEntity>().AsQueryable().Where(o => o.Id.Equals("1") || o.Id.Equals("2")),
                new List<TestEntity>().AsQueryable().Where(o => "1".Equals(o.Id) || o.Id == "2"),
                new List<TestEntity>().AsQueryable().Where(o => "1".Equals(o.Id) || o.Id.Equals("2")),
                new List<TestEntity>().AsQueryable().Where(o => "1".Equals(o.Id) || "2".Equals(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => ids.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => new[] { "1", "2" }.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => ids1.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => new List<string>() { "1", "2" }.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => new List<string>() { obj1.Id, obj2.Id }.Contains(o.Id)),
                new List<TestEntity>().AsQueryable().Where(o => new[] { obj1.Id, obj2.Id }.Contains(o.Id)),
            };
            TestFor(queryables, dataSourceNames);
        }
    }

    public class TestEntity
    {
        public string Id { get; set; }
    }
}