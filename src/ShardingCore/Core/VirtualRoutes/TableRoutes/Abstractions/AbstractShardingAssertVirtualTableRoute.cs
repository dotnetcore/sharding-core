using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/25 17:23:42
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public abstract class AbstractShardingAssertVirtualTableRoute<T, TKey> : AbstractVirtualTableRoute<T, TKey> where T : class, IShardingTable
    {
    }
}
