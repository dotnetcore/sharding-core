using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.CustomerDatabaseProcessers;
using ShardingCore.Core.NotSupportShardingProviders.Abstractions;

namespace ShardingCore.Core.CustomerDatabaseSqlSupports
{
    public class NotSupportAccessor: INotSupportAccessor
    {
        private static AsyncLocal<NotSupportContext> _sqlSupportContext = new AsyncLocal<NotSupportContext>();


        public NotSupportContext SqlSupportContext
        {
            get => _sqlSupportContext.Value;
            set => _sqlSupportContext.Value = value;
        }
    }
}
