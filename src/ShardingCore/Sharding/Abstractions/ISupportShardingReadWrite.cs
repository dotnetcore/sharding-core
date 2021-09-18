using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Sharding.ReadWriteConfigurations;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 20:27:17
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface ISupportShardingReadWrite
    {
        int ReadWriteSeparationPriority { get; set; }
        bool ReadWriteSeparation { get; set; }
    }
}
