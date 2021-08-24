using System;
using System.Collections.Generic;
using System.Text;
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
        public  Dictionary<Type, HashSet<string>> Must { get; }
        public Dictionary<Type, HashSet<string>> Should { get; }
        public Dictionary<Type, HashSet<string>> ShouldNot { get; }

        private ShardingRouteContext()
        {
            Must = new Dictionary<Type, HashSet<string>>();
            Should = new Dictionary<Type, HashSet<string>>();
            ShouldNot = new Dictionary<Type, HashSet<string>>();
        }

        public static ShardingRouteContext Create()
        {
            return new ShardingRouteContext();
        }
    }
}
