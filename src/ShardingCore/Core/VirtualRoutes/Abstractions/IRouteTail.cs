using System;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 22 August 2021 09:39:19
* @Email: 326308290@qq.com
*/
    public interface IRouteTail
    {
        string GetRouteTailIdenty();
        bool IsMultiEntityQuery();
    }
}