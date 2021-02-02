using System;
using ShardingCore.Core;

namespace Samples.AutoByDate.SqlServer.Domain.Entities
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 02 February 2021 17:00:36
* @Email: 326308290@qq.com
*/
    public class SysUserLogByDay:IShardingEntity
    {
        public int Id { get; set; }
        public string Body { get; set; }
        [ShardingKey]
        public DateTime CreateTime { get; set; }
    }
}