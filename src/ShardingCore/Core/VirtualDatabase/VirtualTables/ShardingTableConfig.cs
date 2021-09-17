using System;

namespace ShardingCore.Core.VirtualTables
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 13:24:05
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 分表配置 sharding config
    /// </summary>
    public class ShardingTableConfig
    {
        /// <summary>
        /// 分表类型 sharding entity type
        /// </summary>
        public Type ShardingEntityType { get; set; }

        /// <summary>
        /// 分表字段 sharding field
        /// </summary>
        public string ShardingField { get; set; }

        /// <summary>
        /// 分表的原表名 original table name in db exclude tail
        /// </summary>
        public string ShardingOriginalTable { get; set; }

        /// <summary>
        /// 启动时是否建表 auto create table when start app
        /// </summary>
        public bool? AutoCreateTable { get; set; }

        /// <summary>
        /// 分表尾巴后缀 table sharding tail prefix
        /// </summary>
        public string TailPrefix { get; set; } = "_";
    }
}