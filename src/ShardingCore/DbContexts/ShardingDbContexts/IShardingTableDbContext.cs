using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.Abstractions;

namespace ShardingCore.DbContexts.ShardingDbContexts
{
    /*
   * @Author: xjm
   * @Description:
   * @Date: 2021/3/3 16:15:11
   * @Ver: 1.0
   * @Email: 326308290@qq.com
   */
    public interface IShardingTableDbContext
    {
        /// <summary>
        /// 保存模型时所用
        /// </summary>
        IShardingCruder ShardingCruder {get;}
        /// <summary>
        /// 模型是否需要变更属性 不需要实现框架会自动处理
        /// </summary>
        string ModelChangeKey { get; set; }
    }
}