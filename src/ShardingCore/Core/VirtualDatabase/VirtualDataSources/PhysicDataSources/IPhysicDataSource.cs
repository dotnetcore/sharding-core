using System;
using ShardingCore.Core.VirtualDataSources;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 9:10:53
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IPhysicDataSource
    {
        /// <summary>
        /// data source name
        /// </summary>
        string DSName { get; }
        /// <summary>
        /// 数据源链接
        /// </summary>
        string ConnectionString { get; }
        /// <summary>
        /// 映射类类型
        /// </summary>
        Type EntityType { get; }
        IVirtualDataSource VirtualDataSource { get; }
    }
}
