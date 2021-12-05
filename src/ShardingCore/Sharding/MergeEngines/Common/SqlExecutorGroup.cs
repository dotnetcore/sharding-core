using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    internal sealed class SqlExecutorGroup<T>
    {
        public SqlExecutorGroup(List<T> groups)
        {
            Groups = groups;
        }

        /// <summary>
        /// 执行组
        /// </summary>
        public List<T> Groups { get; }
    }
}
