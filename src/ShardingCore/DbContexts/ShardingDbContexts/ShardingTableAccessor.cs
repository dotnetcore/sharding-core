using System.Threading;
using ShardingCore.Core.VirtualTables;

namespace ShardingCore.DbContexts.ShardingDbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/03/09 13:08:15
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */

    /// <summary>
    /// 
    /// </summary>
    public class ShardingTableAccessor:IShardingTableAccessor
    {
        private static AsyncLocal<ShardingTableContext> _context = new AsyncLocal<ShardingTableContext>();

        /// <inheritdoc />
        public ShardingTableContext Context
        {
            get => _context.Value;
            set => _context.Value = value;
        }
    }
}