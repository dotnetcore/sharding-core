using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    public class VirtualDataSourceScope:IDisposable
    {
        /// <summary>
        /// 分片配置访问器
        /// </summary>
        public IVirtualDataSourceAccessor VirtualDataSourceAccessor { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="virtualDataSourceAccessor"></param>
        public VirtualDataSourceScope(IVirtualDataSourceAccessor virtualDataSourceAccessor)
        {
            VirtualDataSourceAccessor = virtualDataSourceAccessor;
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Dispose()
        {
            VirtualDataSourceAccessor.DataSourceContext = null;
        }
    }
}
