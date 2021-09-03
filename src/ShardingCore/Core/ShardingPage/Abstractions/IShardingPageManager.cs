using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Core.ShardingPage.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 13:46:37
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingPageManager
    {
        ShardingPageContext Current { get; }
        /// <summary>
        /// 创建分页scope
        /// </summary>
        /// <returns></returns>
        ShardingPageScope CreateScope();
    }
}
