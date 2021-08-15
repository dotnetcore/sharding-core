using System;
using System.Collections.Generic;

namespace ShardingCore.Core.Internal.Visitors.Selects
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 02 February 2021 08:17:24
* @Email: 326308290@qq.com
*/
    public class SelectContext
    {
        public List<SelectProperty> SelectProperties { get; set; } = new List<SelectProperty>();
    }
}