using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Core
{
    /// <summary>
    /// 链接模式,可以由用户自行指定，使用内存限制,和连接数限制或者系统自行选择最优
    /// 内存限制的意思是最小化内存使用率，就是非一次性获取所有数据然后采用流式聚合
    /// 连接限制的意思是最小化连接数使用率，就是单次查询并发连接数为设置的连接数。因为有限制，所以无法一直挂起连接，必须全部获取到内存后进行内存聚合
    /// 系统自行选择会根据用户的配置采取最小化连接数，但是如果遇到分页则会根据分页策略采取内存限制，因为skip过大会导致内存爆炸
    /// </summary>
    public enum ConnectionModeEnum
    {
        //内存限制使用流式聚合
        MEMORY_LIMIT,
        //链接限制使用内存聚合
        CONNECTION_LIMIT,
        //系统自动选择内存还是流式聚合
        SYSTEM_AUTO
    }
}
