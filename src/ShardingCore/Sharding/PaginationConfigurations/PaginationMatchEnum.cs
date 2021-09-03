using System;

namespace ShardingCore.Sharding.PaginationConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 01 September 2021 21:27:25
    * @Email: 326308290@qq.com
    */
    [Flags]
    public enum PaginationMatchEnum
    {
        /// <summary>
        /// 必须是当前对象的属性
        /// </summary>
        Owner = 1,
        /// <summary>
        /// 只要名称一样就可以了
        /// </summary>
        Named = 1 << 1,
        /// <summary>
        /// 仅第一个匹配就可以了
        /// </summary>
        PrimaryMatch = 1 << 2
    }
}