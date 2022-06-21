using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Months;
using System;
using WebApplication1.Data.Models;

namespace WebApplication1.Data.Sharding
{
    public class TestModelVirtualTableRoute : AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<TestModel>
    {
        /// <summary>
        /// 固定值不要使用日期时间。现在因为如果如果应用程序重新启动这个值改变的地方
        /// </summary>
        /// <returns></returns>
        public override DateTime GetBeginTime()
        {
            return new DateTime(2022, 1, 1);
        }

        /// <summary>
        /// 配置分片属性
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(EntityMetadataTableBuilder<TestModel> builder)
        {
            builder.ShardingProperty(o => o.CreationTime);
            //builder.AutoCreateTable(null); // 可选,表示是否需要在启动的时候建表:null表示根据全局配置,true:表示需要,false:表示不需要,默认null
            //builder.TableSeparator("_"); // 可选,表示分表后缀和虚拟表名之间的分隔连接符,默认_
        }

        /// <summary>
        /// 启用自动创建表的工作
        /// </summary>
        /// <returns></returns>
        public override bool AutoCreateTableByTime()
        {
            return true;
        }
    }
}
