using System;
using System.Collections.Generic;

namespace ShardingCore.Core.Internal
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 13:34:29
* @Email: 326308290@qq.com
*/
    public class ShardingEntityBaseType
    {
        /// <summary>
        /// 数据源
        /// </summary>
        public Type EntityType { get; set; }
        /// <summary>
        /// 是否多数据源
        /// </summary>
        public bool IsMultiDataSourceMapping { get; set; }
        /// <summary>
        /// 是否分表
        /// </summary>
        public bool IsMultiTableMapping { get;set;  }
        /// <summary>
        /// 分库字段
        /// </summary>
        public string ShardingDataSourceField { get;set;  }
        /// <summary>
        /// 分表字段
        /// </summary>
        public string ShardingTableField { get; set; }
        /// <summary>
        /// 分表的原表名 original table name in db exclude tail
        /// </summary>
        public string ShardingOriginalTableName { get; set; }

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