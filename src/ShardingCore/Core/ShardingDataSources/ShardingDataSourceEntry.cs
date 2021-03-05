using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;

namespace ShardingCore.Core.ShardingDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 18 February 2021 17:13:16
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 分库对象
    /// </summary>
    public class ShardingDataSourceEntry
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="dataSourceDbEntry"></param>
        public ShardingDataSourceEntry(Type entityType, ShardingDataSourceDbEntry dataSourceDbEntry)
        {
            EntityType = entityType;
            DataSourceDbEntry = dataSourceDbEntry;
        }

        /// <summary>
        /// 分库类型
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// 分库对应的数据库对象
        /// </summary>
        public ShardingDataSourceDbEntry DataSourceDbEntry { get; }

        public override int GetHashCode()
        {
            return this.EntityType.GetHashCode() ^ 31;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ShardingDataSourceEntry))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ShardingDataSourceEntry item = (ShardingDataSourceEntry) obj;

            return item.EntityType == this.EntityType;
        }
    }
}