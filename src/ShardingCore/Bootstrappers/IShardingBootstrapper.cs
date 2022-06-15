/*
* @Author: xjm
* @Description:
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrappers
{
    /// <summary>
    /// 主要的分表启动器
    /// </summary>
    public interface IShardingBootstrapper
    {
        /// <summary>
        /// 启动
        /// </summary>
        void Start();
    }
}