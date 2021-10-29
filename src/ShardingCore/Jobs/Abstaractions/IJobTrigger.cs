using System;

namespace ShardingCore.Jobs.Abstaractions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 08 January 2021 22:18:30
* @Email: 326308290@qq.com
*/
    public interface IJobTrigger
    {
        DateTime NextJobRunUtcTime();
    }
}