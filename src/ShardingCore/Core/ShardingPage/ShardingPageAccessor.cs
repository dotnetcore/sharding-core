using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ShardingCore.Core.ShardingPage.Abstractions;

namespace ShardingCore.Core.ShardingPage
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 13:47:35
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingPageAccessor:IShardingPageAccessor
    {
        private static AsyncLocal<ShardingPageContext> _shardingPageContext = new AsyncLocal<ShardingPageContext>();


        public ShardingPageContext ShardingPageContext
        {
            get => _shardingPageContext.Value;
            set => _shardingPageContext.Value = value;
        }
    }
}
