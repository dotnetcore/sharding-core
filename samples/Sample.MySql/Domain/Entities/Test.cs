using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.VirtualRoutes.Abstractions;

namespace Sample.MySql.Domain.Entities
{
   
    [Table("test", Schema = "test")]
    public class Test
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    
        public DateTime UtcTime { get; set; }
   
    } 
    public abstract class AbstractShardingQuarterKeyDateTimeVirtualTableRoute<TEntity> : AbstractShardingTimeKeyDateTimeVirtualTableRoute<TEntity> where TEntity : class
    {
        public override bool AutoCreateTableByTime()
        {
            return false;
        }
        public abstract DateTime GetBeginTime();
        private static DateTime GetCurrentQuarterFirstDay([NotNull] DateTime time)
        {
            return time.Date.AddMonths(0 - (time.Month - 1) % 3).AddDays(1 - time.Day);
        }
        private static DateTime GetNextQuarterFirstDay([NotNull] DateTime time)
        {
            return GetCurrentQuarterFirstDay(time).AddMonths(3);
        }
        protected override List<string> CalcTailsOnStart()
        {
            var beginTime = GetCurrentQuarterFirstDay(GetBeginTime());

            var tails = new List<string>();
            //提前创建表
            var nowTimeStamp = GetCurrentQuarterFirstDay(DateTime.Now);
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("begin time error");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var tail = ShardingKeyToTail(currentTimeStamp);
                tails.Add(tail);
                currentTimeStamp = GetNextQuarterFirstDay(currentTimeStamp);
            }
            return tails;
        }
        protected override string TimeFormatToTail(DateTime time)
        {
            var quarter = (time.Month - 1) / 3 + 1;
            return $"{time:yyyy}0{quarter}";
        }
        public override Func<string, bool> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => string.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                    {
                        var currentQuarter = GetCurrentQuarterFirstDay(shardingKey);
                        //处于临界值 o=>o.time < [2021-01-01 00:00:00] 尾巴20210101不应该被返回
                        if (currentQuarter == shardingKey)
                            return tail => string.Compare(tail, t, StringComparison.Ordinal) < 0;
                        return tail => string.Compare(tail, t, StringComparison.Ordinal) <= 0;
                    }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail => string.Compare(tail, t, StringComparison.Ordinal) <= 0;
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");
                    return tail => true;
            }
        }
        public override string[] GetCronExpressions()
        {
            return new[]
            {
                "0 59 23 28,29,30,31 1,4,7,10 ?",
                "0 0 0 1 1,4,7,10 ?",
                "0 1 0 1 1,4,7,10 ?",
            };
        }
        public override string[] GetJobCronExpressions()
        {
            return base.GetJobCronExpressions().Concat(new[] { "0 0 0 1 1,4,7,10 ?" }).Distinct().ToArray();
        }
    }
    public class TestTableRoute : AbstractShardingQuarterKeyDateTimeVirtualTableRoute<Test>
    {
        protected override bool EnableHintRoute => true;

        public override DateTime GetBeginTime()
        {
            return new DateTime(2023, 1, 1);
        }

        public override void Configure(EntityMetadataTableBuilder<Test> builder)
        {
            builder.ShardingProperty(x => x.UtcTime);
        }
    }
}
