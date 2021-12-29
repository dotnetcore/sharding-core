using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.QueryRouteManagers
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/23 16:55:33
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingRouteContext
    {
        #region 分库提示路由
        /// <summary>
        /// 强制路由直接返回对应的后缀表
        /// </summary>
        public Dictionary<Type, HashSet<string>> MustDataSource { get; }
        public ISet<string> MustAllDataSource { get; }
        /// <summary>
        /// 提示路由会经过断言的强制路由
        /// </summary>
        public Dictionary<Type, HashSet<string>> HintDataSource { get; }
        public ISet<string> HintAllDataSource { get; }
        /// <summary>
        /// 断言
        /// </summary>
        public Dictionary<Type, LinkedList<IDataSourceRouteAssert>> AssertDataSource { get; }
        public LinkedList<IDataSourceRouteAssert> AssertAllDataSource { get; }
        #endregion

        #region 分表提示路由
        /// <summary>
        /// 强制路由直接返回对应的后缀表
        /// </summary>
        public Dictionary<Type, HashSet<string>> MustTable { get; }
        /// <summary>
        /// 提示路由会经过断言的强制路由
        /// </summary>
        public Dictionary<Type, HashSet<string>> HintTable { get; }
        /// <summary>
        /// 断言
        /// </summary>
        public Dictionary<Type, LinkedList<ITableRouteAssert>> AssertTable { get; } 
        #endregion
        private ShardingRouteContext()
        {
            MustDataSource = new Dictionary<Type, HashSet<string>>();
            MustAllDataSource = new HashSet<string>();
            HintDataSource = new Dictionary<Type, HashSet<string>>();
            HintAllDataSource = new HashSet<string>();
            AssertDataSource = new Dictionary<Type, LinkedList<IDataSourceRouteAssert>>();
            AssertAllDataSource = new LinkedList<IDataSourceRouteAssert>();
            MustTable = new Dictionary<Type, HashSet<string>>();
            HintTable = new Dictionary<Type, HashSet<string>>();
            AssertTable = new Dictionary<Type, LinkedList<ITableRouteAssert>>();
        }

        public static ShardingRouteContext Create()
        {
            return new ShardingRouteContext();
        }
    }
}
