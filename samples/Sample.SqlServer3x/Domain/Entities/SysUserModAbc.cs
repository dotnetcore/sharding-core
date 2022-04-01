namespace Sample.SqlServer3x.Domain.Entities
{
    public class SysUserModAbc
    {
        /// <summary>
        /// 用户Id用于分表
        /// </summary>
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
