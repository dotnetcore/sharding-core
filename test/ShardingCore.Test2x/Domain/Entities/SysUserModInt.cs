namespace ShardingCore.Test2x.Domain.Entities
{
    public class SysUserModInt
    {
        /// <summary>
        /// 用户Id用于分表
        /// </summary>
        public int Id { get; set; }
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
