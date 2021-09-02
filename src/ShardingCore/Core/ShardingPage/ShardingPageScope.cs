using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.ShardingPage.Abstractions;

namespace ShardingCore.Core.ShardingPage
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 13:49:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingPageScope : IDisposable
    {

        /// <summary>
        /// 分表配置访问器
        /// </summary>
        public IShardingPageAccessor ShardingPageAccessor { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="shardingPageAccessor"></param>
        public ShardingPageScope(IShardingPageAccessor shardingPageAccessor)
        {
            ShardingPageAccessor = shardingPageAccessor;
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Dispose()
        {
            ShardingPageAccessor.ShardingPageContext = null;
        }
    }
}