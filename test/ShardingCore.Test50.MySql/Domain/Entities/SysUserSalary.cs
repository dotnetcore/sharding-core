using System;
using ShardingCore.Core;

namespace ShardingCore.Test50.MySql.Domain.Entities
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 01 February 2021 15:40:46
* @Email: 326308290@qq.com
*/
    public class SysUserSalary:IShardingEntity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        /// <summary>
        /// 每月的金额
        /// </summary>
        [ShardingKey(AutoCreateTableOnStart = false)]
        public int DateOfMonth { get; set; }
        /// <summary>
        /// 工资
        /// </summary>
        public int Salary { get; set; }
    }
}