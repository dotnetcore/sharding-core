using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 16:52:29
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingReadWriteContext
    {
        public bool DefaultReadEnable { get; set; }
        public int DefaultPriority { get; set; }

        private ShardingReadWriteContext()
        {
            DefaultReadEnable = false;
            DefaultPriority = 0;
        }

        public static ShardingReadWriteContext Create()
        {
            return new ShardingReadWriteContext();
        }
    }
}
