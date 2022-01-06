using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions
{
    /// <summary>
    /// 数据源池
    /// </summary>
    public interface IPhysicDataSourcePool
    {
        /// <summary>
        /// 添加一个物理数据源
        /// </summary>
        /// <param name="physicDataSource"></param>
        /// <returns></returns>
        bool TryAdd(IPhysicDataSource physicDataSource);
        /// <summary>
        /// 尝试获取一个物理数据源没有返回null
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        IPhysicDataSource TryGet(string dataSourceName);
        /// <summary>
        /// 获取所有的数据源名称
        /// </summary>
        /// <returns></returns>
        List<string> GetAllDataSourceNames();

        IDictionary<string, string> GetDataSources();
    }
}
