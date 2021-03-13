using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/13 8:17:41
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IDbContextCreateFilter
    {
        /// <summary>
        /// dbContext创建完成后
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="s"></param>
        void CreateAfter(DbContext dbContext, IServiceProvider s);
    }
}
