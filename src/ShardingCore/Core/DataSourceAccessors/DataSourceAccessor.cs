using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Core.DataSourceAccessors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/1 16:32:30
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 创建数据库数据
    /// </summary>
    public class DataSourceAccessor: IDataSourceAccessor
    {
        public DbContext CreateDbContext()
        {
            throw new NotImplementedException();
        }
    }
}
