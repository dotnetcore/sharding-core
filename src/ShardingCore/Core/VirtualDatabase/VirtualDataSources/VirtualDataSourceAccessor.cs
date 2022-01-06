using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    public class VirtualDataSourceAccessor: IVirtualDataSourceAccessor
    {
        private static AsyncLocal<VirtualDataSourceContext> _shardingConfigurationContext = new AsyncLocal<VirtualDataSourceContext>();

        /// <summary>
        /// sharding route context use in using code block
        /// </summary>
        public VirtualDataSourceContext DataSourceContext
        {
            get => _shardingConfigurationContext.Value;
            set => _shardingConfigurationContext.Value = value;
        }
    }
}
