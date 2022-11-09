#if NETCOREAPP2_0 && SHARDINGCORE2_6
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/20 17:05:36
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */

    
    public class ShardingInternalDbQuery<TQuery> : InternalDbQuery<TQuery> where TQuery : class
    {
        public ShardingInternalDbQuery(DbContext context) : base(context)
        {
        }
    }

}
#endif
