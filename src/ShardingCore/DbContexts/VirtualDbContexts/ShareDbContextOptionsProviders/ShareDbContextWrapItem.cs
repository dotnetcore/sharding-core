using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.VirtualDbContexts.ShareDbContextOptionsProviders
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/5 9:04:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShareDbContextWrapItem
    {
        public ShareDbContextWrapItem(DbConnection connection, DbContextOptions contextOptions)
        {
            Connection = connection;
            ContextOptions = contextOptions;
        }

        public string ConnectKey { get;  }
        public DbConnection Connection { get; }
        public DbContextOptions ContextOptions { get; }
    }
}
