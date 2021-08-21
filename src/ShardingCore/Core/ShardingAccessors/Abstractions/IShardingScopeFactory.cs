namespace ShardingCore.Core.ShardingAccessors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 23 December 2020 07:51:00
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 查询scope创建
    /// </summary>
    public interface IShardingScopeFactory
    {
        /// <summary>
        /// 创建查询scope
        /// </summary>
        /// <returns></returns>
        ShardingScope CreateScope();
    }
}