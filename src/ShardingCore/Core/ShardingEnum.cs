using System;

namespace ShardingCore.Core
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 13:07:31
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 数据源类型
    /// </summary>
    public enum DataSourceEnum
    {
        /// <summary>
        /// SqlServer 1
        /// </summary>
        SqlServer = 1,

        /// <summary>
        /// MySql 2
        /// </summary>
        MySql = 1 << 1
    }
}