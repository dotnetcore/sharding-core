using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Core.VirtualDatabase
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 9:30:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingEntityConfig
    {
        /// <summary>
        /// 分表类型 sharding entity type
        /// </summary>
        public Type EntityType { get; set; }
        /// <summary>
        /// 是否多数据源
        /// </summary>
        public bool IsMultiDataSourceMapping { get; set; }
        /// <summary>
        /// 是否分表
        /// </summary>
        public bool IsMultiTableMapping { get; set; }
        /// <summary>
        /// 分库字段
        /// </summary>
        public string ShardingDataSourceField { get; set; }

        /// <summary>
        /// 启动时是否建表 auto create data source when start app
        /// </summary>
        public bool? AutoCreateDataSourceTable { get; set; }

        /// <summary>
        /// 分表字段 sharding table field
        /// </summary>
        public string ShardingTableField { get; set; }

        /// <summary>
        /// 分表的原表名 original table name in db exclude tail
        /// </summary>
        public string VirtualTableName { get; set; }

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
