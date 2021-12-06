using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core;

namespace ShardingCore.Sharding.MergeEngines.Common
{
    internal sealed class SqlExecutorGroup<T>
    {
        public SqlExecutorGroup(ConnectionModeEnum connectionMode,List<T> groups)
        {
            ConnectionMode = connectionMode;
            Groups = groups;
        }

        public ConnectionModeEnum ConnectionMode { get; }
        /// <summary>
        /// 执行组
        /// </summary>
        public List<T> Groups { get; }

    }
}
