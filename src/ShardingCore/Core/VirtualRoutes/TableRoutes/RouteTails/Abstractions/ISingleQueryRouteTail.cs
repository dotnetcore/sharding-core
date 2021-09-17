namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 22 August 2021 09:44:22
* @Email: 326308290@qq.com
*/
    public interface ISingleQueryRouteTail:IRouteTail
    {
        /// <summary>
        /// 获取当前查询的后缀
        /// </summary>
        /// <returns></returns>
        string GetTail();
    }
}