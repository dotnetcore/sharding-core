using System;

namespace ShardingCore.Core
{
/*
* @Author: xjm
* @Description:用户数据库分数据源时进行判断
* @Date: Friday, 05 February 2021 12:53:46
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 数据源分库额外规则字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ShardingExtraDataSourceKeyAttribute : Attribute
    {
    }
}