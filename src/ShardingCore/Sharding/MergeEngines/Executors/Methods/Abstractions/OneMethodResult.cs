using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/21 12:56:20
    /// Email: 326308290@qq.com
    public class OneMethodResult<TEntity>
    {
        public OneMethodResult(TEntity entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// 查询结果
        /// </summary>
        public TEntity Entity { get; }
    }
}
