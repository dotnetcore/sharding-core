using System.ComponentModel;

namespace ShardingCore.Core.VirtualRoutes
{
    /// <summary>
    /// 分片条件比较符
    /// </summary>
    public enum ShardingOperatorEnum
    {
        /// <summary>
        /// 未知操作符
        /// </summary>
        [Description("??")] UnKnown,

        /// <summary>
        /// 大于
        /// </summary>
        [Description(">")] GreaterThan,

        /// <summary>
        /// 大于等于
        /// </summary>
        [Description(">=")] GreaterThanOrEqual,

        /// <summary>
        /// 小于
        /// </summary>
        [Description("<")] LessThan,

        /// <summary>
        /// 小于等于
        /// </summary>
        [Description("<=")] LessThanOrEqual,

        /// <summary>
        /// 等于
        /// </summary>
        [Description("==")] Equal,

        /// <summary>
        /// 不等于
        /// </summary>
        [Description("!=")] NotEqual,

        /// <summary>
        /// like 类似 contains
        /// </summary>
        [Description("%w%")] AllLike,

        /// <summary>
        /// like 类似 start with
        /// </summary>
        [Description("w%")] StartLike,

        /// <summary>
        /// like 类似 end with
        /// </summary>
        [Description("%w")] EndLike
    }
}