using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    public class VirtualDataSourceContext
    {
        public string ConfigId { get; }

        public VirtualDataSourceContext(string configId)
        {
            ConfigId = configId;
        }
    }
}
