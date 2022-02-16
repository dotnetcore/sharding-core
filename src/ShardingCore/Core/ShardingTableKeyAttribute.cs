using System;

namespace ShardingCore.Core
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 16 December 2020 11:04:51
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// AbstractVirtualTableRoute 最基础分表规则 需要自己解析如何分表
    /// 仅ShardingMode为Custom:以下接口提供自定义分表
    /// AbstractShardingKeyObjectEqualVirtualRoute 自定义分表 
    /// SimpleShardingKeyStringModVirtualRoute 默认对AbstractShardingKeyObjectEqualVirtualRoute的实现,字符串按取模分表
    /// 仅ShardingMode非Custom：以下接口提供自动按时间分表
    /// SimpleShardingDateByDayVirtualRoute 分表 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ShardingTableKeyAttribute : Attribute
    {
        public const string DEFAULT_TABLE_SEPARATOR = "_";

        /// <summary>
        /// 是否需要在启动的时候创建表
        /// </summary>
        public ShardingKeyAutoCreateTableEnum AutoCreateTableOnStart { get; set; } = ShardingKeyAutoCreateTableEnum.UnKnown;

        /// <summary>
        /// 分表尾巴前缀 
        /// </summary>
        public string TableSeparator { get; set; } = DEFAULT_TABLE_SEPARATOR;
    }
}