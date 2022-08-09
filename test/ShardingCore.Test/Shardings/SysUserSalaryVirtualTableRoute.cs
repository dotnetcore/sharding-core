using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Test.Domain.Entities;

namespace ShardingCore.Test.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 01 February 2021 15:54:55
* @Email: 326308290@qq.com
*/
    public class SysUserSalaryVirtualTableRoute:AbstractShardingOperatorVirtualTableRoute<SysUserSalary,int>
    {
        //public override bool? EnableRouteParseCompileCache => true;
        protected override bool EnableHintRoute => true;

        public override string ShardingKeyToTail(object shardingKey)
        {
            var time = Convert.ToInt32(shardingKey);
            return TimeFormatToTail(time);
        }

        public override List<string> GetTails()
        {
            var beginTime = new DateTime(2020, 1, 1);
            var endTime = new DateTime(2021, 12, 1);
            var list = new List<string>(24);
            var tempTime = beginTime;
            while (tempTime <= endTime)
            {
                list.Add($"{tempTime:yyyyMM}");
                tempTime = tempTime.AddMonths(1);
            }

            return list;
        }

        protected  string TimeFormatToTail(int time)
        {
            var dateOfMonth=DateTime.ParseExact($"{time}","yyyyMM",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.AdjustToUniversal);
            return $"{dateOfMonth:yyyyMM}";
        }

        public override Func<string, bool> GetRouteToFilter(int shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) < 0;
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
#if DEBUG
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");
#endif
                    return tail => true;
                }
            }
        }

        public override void Configure(EntityMetadataTableBuilder<SysUserSalary> builder)
        {
            builder.ShardingProperty(o => o.DateOfMonth);
        }
    }
}