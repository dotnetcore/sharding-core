using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Extensions;

namespace ShardingCore.SqlServer
{
/*
* @Author: xjm
* @Description:
* @Date: 2020年4月7日 8:34:04
* @Email: 326308290@qq.com
*/
    public class SqlServerOptions
    {
        public LinkedList<Type> ShardingRoutes=new LinkedList<Type>();
        public  string ConnectionString { get; set; }

        public void AddSharding<TRoute>()where TRoute:IVirtualRoute
        {
            ShardingRoutes.AddLast(typeof(TRoute));
        }

        public bool HasSharding => ShardingRoutes.IsNotEmpty();
        /// <summary>
        /// 创建表如果数据库不存在的话，创建的表是非sharding表
        /// </summary>
        public bool EnsureCreated { get; set; }
    }
}