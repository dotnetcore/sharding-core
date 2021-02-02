namespace ShardingCore
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 14 January 2021 15:50:46
* @Email: 326308290@qq.com
*/
    public class ShardingCoreConfig
    {
        /// <summary>
        /// 如果数据库不存在就创建并且创建表
        /// </summary>
        public bool EnsureCreated { get; set; }
        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }
    }
}