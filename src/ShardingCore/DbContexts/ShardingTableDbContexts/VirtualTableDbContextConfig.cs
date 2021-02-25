using System;

namespace ShardingCore.DbContexts.ShardingTableDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 01 January 2021 16:24:57
* @Email: 326308290@qq.com
*/
    public class VirtualTableDbContextConfig
    {
        public VirtualTableDbContextConfig(Type shardingEntityType, string originalTableName, string tailPrefix)
        {
            ShardingEntityType = shardingEntityType;
            OriginalTableName = originalTableName;
            TailPrefix = tailPrefix;
        }

        /// <summary>
        /// 分表实体类型
        /// </summary>
        public Type ShardingEntityType { get; }
        
        /// <summary>
        /// 原始表名不带后缀
        /// </summary>
        public string OriginalTableName { get; }
        /// <summary>
        /// 表尾巴前缀
        /// </summary>
        public string TailPrefix { get; }
    }
}