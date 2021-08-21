namespace ShardingCore.Core.ShardingAccessors.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 22 December 2020 15:13:44
* @Email: 326308290@qq.com
*/
    public interface IShardingAccessor
    {
        ShardingContext ShardingContext { get; set; }
    }
}