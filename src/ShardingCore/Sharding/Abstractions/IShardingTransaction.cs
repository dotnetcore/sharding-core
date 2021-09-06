using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 8:41:50
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingTransaction
    {
        void UseShardingTransaction(DbTransaction transaction);
    }
}
