using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.NotSupportShardingProviders.Abstractions;

namespace ShardingCore.Core.CustomerDatabaseSqlSupports
{
    public class NotSupportScope:IDisposable
    {
        public INotSupportAccessor NotSupportAccessor { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="notSupportAccessor"></param>
        public NotSupportScope(INotSupportAccessor notSupportAccessor)
        {
            NotSupportAccessor = notSupportAccessor;
        }
        public void Dispose()
        {
            NotSupportAccessor.SqlSupportContext = null;
        }
    }
}
