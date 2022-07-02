namespace ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions
{
    /// <summary>
    /// 可以追加尾巴
    /// </summary>
    public interface ITailAppendable
    {
       /// <summary>
       /// 追加尾巴
       /// </summary>
       /// <param name="tail"></param>
       /// <returns>是否添加成功</returns>
        bool Append(string tail);
    }
}
