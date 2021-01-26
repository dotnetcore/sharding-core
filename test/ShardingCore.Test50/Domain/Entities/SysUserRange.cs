using ShardingCore.Core;

namespace ShardingCore.Test50.Domain.Entities
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 20 January 2021 10:43:19
* @Email: 326308290@qq.com
*/
    public class SysUserRange:IShardingEntity
    {
        /// <summary>
        /// 分表分库range切分
        /// </summary>
        [ShardingKey(TailPrefix = "_",AutoCreateTableOnStart = true)]
        public string Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }
    }
}