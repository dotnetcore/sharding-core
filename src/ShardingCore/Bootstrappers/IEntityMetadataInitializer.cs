

/*
* @Author: xjm
* @Description:
* @Ver: 1.0
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Bootstrappers
{
    /// <summary>
    /// 元数据对象初始化器
    /// </summary>
    public interface IEntityMetadataInitializer
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
    }
}
