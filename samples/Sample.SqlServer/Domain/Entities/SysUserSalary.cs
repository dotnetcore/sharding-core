using ShardingCore.Core;

namespace Sample.SqlServer.Domain.Entities
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 01 February 2021 15:43:22
* @Email: 326308290@qq.com
*/
    public class SysUserSalary
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        /// <summary>
        /// 每月的金额
        /// </summary>
        public int DateOfMonth { get; set; }
        /// <summary>
        /// 工资
        /// </summary>
        public int Salary { get; set; }
        /// <summary>
        /// 工资
        /// </summary>
        public long SalaryLong { get; set; }
        
        /// <summary>
        /// 工资
        /// </summary>
        public decimal SalaryDecimal { get; set; }
        /// <summary>
        /// 工资
        /// </summary>
        public double SalaryDouble { get; set; }
        /// <summary>
        /// 工资
        /// </summary>
        public float SalaryFloat { get; set; }
    }
}