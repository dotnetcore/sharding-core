using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Core.QueryRouteManagers.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/22 7:54:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 路由断言
    /// </summary>
    public interface IDataSourceRouteAssert
    {
        /// <summary>
        /// 断言路由结果
        /// </summary>
        /// <param name="allDataSources">所有的路由数据源</param>
        /// <param name="resultDataSources">本次查询路由返回结果</param>
        void Assert(List<string> allDataSources, List<string> resultDataSources);
    }

    public interface IDataSourceRouteAssert<T> : IDataSourceRouteAssert where T : class
    {

    }
}
