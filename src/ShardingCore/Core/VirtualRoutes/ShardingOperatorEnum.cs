using System.ComponentModel;

namespace ShardingCore.Core.VirtualRoutes
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 19:56:57
* @Email: 326308290@qq.com
*/
    public enum ShardingOperatorEnum
    {
        [Description("??")]
        UnKnown,
        [Description(">")]
        GreaterThan,
        [Description(">=")]
        GreaterThanOrEqual,
        [Description("<")]
        LessThan,
        [Description("<=")]
        LessThanOrEqual,
        [Description("==")]
        Equal,
        [Description("!=")]
        NotEqual,
        // [Description("%w%")]
        // AllLike,
        // [Description("%w")]
        // StartLike,
        // [Description("w%")]
        // EndLike
        // [Description("Contains")]
        // BeContains
    }
}