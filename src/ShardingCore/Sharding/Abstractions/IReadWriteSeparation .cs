using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Abstractions
{
    /// <summary>
    /// 读写分离
    /// </summary>
    /// Author: xjm
    /// Created: 2022/3/30 14:11:06
    /// Email: 326308290@qq.com
    public interface IReadWriteSeparation
    {
        int ReadWriteSeparationPriority { get; set; }
        bool ReadWriteSeparation { get; set; }
    }
}
