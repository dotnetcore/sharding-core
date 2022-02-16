using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Abstractions
{
    public interface ISeqQueryProvider
    {
        /// <summary>
        /// 是否是顺序查询
        /// </summary>
        /// <returns></returns>
        bool IsSeqQuery();
        /// <summary>
        /// 是否可以终端:本次查询n张表,链接数限制m,当n>m时则会出现串行查询才需要中断
        /// </summary>
        /// <returns></returns>
        bool CanTrip();
    }
}
