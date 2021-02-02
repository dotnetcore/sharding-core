using System;
using System.Collections.Generic;
using Samples.AutoByDate.SqlServer.Domain.Entities;
using ShardingCore.VirtualRoutes.Days;

namespace Samples.AutoByDate.SqlServer.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 02 February 2021 17:14:53
* @Email: 326308290@qq.com
*/
    public class SysUserLogByDayVirtualRoute:AbstractSimpleShardingDayKeyDateTimeVirtualRoute<SysUserLogByDay>
    {
        public override List<string> GetAllTails()
        {
            var beginTime = DateTime.Now.AddDays(-2);
         
            var tails=new List<string>();
            //提前创建表
            var nowTimeStamp = DateTime.Now.AddDays(1).Date;
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("起始时间不正确无法生成正确的表名");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var tail = ShardingKeyToTail(currentTimeStamp);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddDays(1);
            }
            return tails;
        }
    }
}