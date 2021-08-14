namespace ShardingCore.DbContexts.ShardingDbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/03/09 13:04:52
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */

    /// <summary>
    /// 分表上下文
    /// </summary>
    public class ShardingTableContext
    {
        private ShardingTableContext(string tail)
        {
            Tail = tail;
        }
        public static ShardingTableContext Create(string tail)
        {
            return new ShardingTableContext(tail);
        }
        /// <summary>
        /// 尾巴
        /// </summary>
        public string Tail { get; set; }
    }
}