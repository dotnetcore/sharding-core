using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.Visitors.Selects;

namespace ShardingCore.Sharding.MergeContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 02 February 2021 08:17:24
* @Email: 326308290@qq.com
*/
    public class SelectContext
    {
        public List<SelectOwnerProperty> SelectProperties { get;  } = new List<SelectOwnerProperty>();

        public bool HasAverage()
        {
            return SelectProperties.Any(o => o is SelectAverageProperty);
        }

        public bool HasCount()
        {
            return SelectProperties.Any(o=>o is SelectCountProperty);
        }

        public override string ToString()
        {
            return String.Join(",",SelectProperties.Select(o=>$"{o}"));
        }
    }
}