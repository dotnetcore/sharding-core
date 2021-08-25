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
        /// <summary>
        /// 强制路由直接返回对应的后缀表
        /// </summary>
        public  Dictionary<Type, HashSet<string>> Must { get; }
        /// <summary>
        /// 提示路由会经过断言的强制路由
        /// </summary>
        public  Dictionary<Type, HashSet<string>> Hint { get; }
        /// <summary>
        /// 断言
        /// </summary>
        public Dictionary<Type, LinkedList<IRouteAssert>> Assert { get; }
        private ShardingRouteContext()
        {
            Must = new Dictionary<Type, HashSet<string>>();
            Hint = new Dictionary<Type, HashSet<string>>();
            Assert = new Dictionary<Type, LinkedList<IRouteAssert>>();
        }

        public static ShardingRouteContext Create()
        {
            return new ShardingRouteContext();
        }
    }
}
