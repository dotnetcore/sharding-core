using ShardingCore.Core;

namespace ShardingCore.Test50.Domain.Entities
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 14 January 2021 15:36:43
* @Email: 326308290@qq.com
*/
    public class SysUserMod:IShardingTable
    {
        /// <summary>
        /// 用户Id用于分表
        /// </summary>
        [ShardingKey(TailPrefix = "_")]
        public string Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>
        public int Age { get; set; }
        public int AgeGroup { get; set; }
        
    }
}