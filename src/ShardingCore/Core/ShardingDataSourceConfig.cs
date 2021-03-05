//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ShardingCore.Core
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: 2021/3/2 15:06:18
//    * @Ver: 1.0
//    * @Email: 326308290@qq.com
//    */
//    public class ShardingDataSourceConfig
//    {
//        public ShardingDataSourceConfig(string connectKey, string connectionString)
//        {
//            ConnectKey = connectKey;
//            ConnectionString = connectionString;
//            Entities = new HashSet<Type>();
//        }

//        /// <summary>
//        /// 连接标识
//        /// </summary>
//        public string ConnectKey { get; }

//        /// <summary>
//        /// 连接字符串
//        /// </summary>
//        public string ConnectionString { get; }

//        public ISet<Type> Entities { get; }

//        public override int GetHashCode()
//        {
//            return this.ConnectKey.GetHashCode() ^ 31;
//        }

//        public override bool Equals(object obj)
//        {
//            if (!(obj is ShardingDataSourceConfig))
//                return false;

//            if (ReferenceEquals(this, obj))
//                return true;

//            ShardingDataSourceConfig item = (ShardingDataSourceConfig)obj;

//            return item.ConnectKey == this.ConnectKey;
//        }
//    }
//}
