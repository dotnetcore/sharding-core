using System;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Core.DataSourceAccessors
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 25 February 2021 20:19:44
* @Email: 326308290@qq.com
*/
    public interface IDataSourceAccessor
    {
        DbContext CreateDbContext();
    }
}