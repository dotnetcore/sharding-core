using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DynamicDataSources
{
    public interface IDataSourceInitializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceName"></param>
        void InitConfigure( string dataSourceName);
    }
}
