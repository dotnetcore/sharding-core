using System;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.VirtualDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 24 December 2020 10:32:07
* @Email: 326308290@qq.com
*/
    public interface IDbContextOptionsProvider:IDisposable
    {
        /// <summary>
        /// 创建数据库链接配置
        /// </summary>
        /// <param name="connectKey"></param>
        /// <returns></returns>
        DbContextOptions GetDbContextOptions(string connectKey);
    }
}