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
    /// 分片额外配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ShardingExtraTableKeyAttribute : Attribute
    {
    }
}