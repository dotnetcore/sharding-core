using System;
using ShardingCore.Core.ShardingAccessors.Abstractions;

namespace ShardingCore.Core.ShardingAccessors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 23 December 2020 07:51:30
* @Email: 326308290@qq.com
*/
    public class ShardingScope : IDisposable
    {

        /// <summary>
        /// 分表配置访问器
        /// </summary>
        public IShardingAccessor ShardingAccessor { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="shardingAccessor"></param>
        public ShardingScope(IShardingAccessor shardingAccessor)
        {
            shardingAccessor.ShardingContext = null;
            ShardingAccessor = shardingAccessor;
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Dispose()
        {
            ShardingAccessor.ShardingContext = null;
        }
    }
}