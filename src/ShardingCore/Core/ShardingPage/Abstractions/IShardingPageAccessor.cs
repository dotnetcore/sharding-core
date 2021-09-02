using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Core.ShardingPage.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 13:46:13
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingPageAccessor
    {
        ShardingPageContext ShardingPageContext { get; set; }
    }
}
