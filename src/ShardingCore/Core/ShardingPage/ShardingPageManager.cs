using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.ShardingPage.Abstractions;

namespace ShardingCore.Core.ShardingPage
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 13:50:13
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingPageManager: IShardingPageManager
    {
        private readonly IShardingPageAccessor _shardingPageAccessor;

        public ShardingPageManager(IShardingPageAccessor shardingPageAccessor)
        {
            _shardingPageAccessor = shardingPageAccessor;
        }

        public ShardingPageContext Current => _shardingPageAccessor.ShardingPageContext;
        public ShardingPageScope CreateScope()
        {
            var shardingPageScope = new ShardingPageScope(_shardingPageAccessor);
            _shardingPageAccessor.ShardingPageContext = ShardingPageContext.Create();
            return shardingPageScope;
        }
    }
}
