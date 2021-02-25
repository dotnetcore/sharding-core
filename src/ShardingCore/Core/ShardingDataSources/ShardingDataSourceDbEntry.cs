using System;

namespace ShardingCore.Core.ShardingDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 20 February 2021 09:13:56
* @Email: 326308290@qq.com
*/
    public class ShardingDataSourceDbEntry
    {
        public ShardingDataSourceDbEntry(string connectKey, Type dbContextType, string connectionString)
        {
            ConnectKey = connectKey;
            DbContextType = dbContextType;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// 连接标识
        /// </summary>
        public string ConnectKey { get; }

        /// <summary>
        /// dbcontext类型
        /// </summary>
        public Type DbContextType { get; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; }

        public override int GetHashCode()
        {
            return this.ConnectKey.GetHashCode() ^ 31;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ShardingDataSourceDbEntry))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            ShardingDataSourceDbEntry item = (ShardingDataSourceDbEntry) obj;

            return item.ConnectKey == this.ConnectKey;
        }
    }
}