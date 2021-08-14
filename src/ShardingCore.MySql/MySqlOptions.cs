using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ShardingCore.MySql
{
/*
* @Author: xjm
* @Description:
* @Date: 2020年4月7日 8:34:04
* @Email: 326308290@qq.com
*/
    public class MySqlOptions: AbstractShardingCoreOptions
    {
#if EFCORE5
        public MySqlServerVersion ServerVersion { get; set; }
#endif
        
        public Action<MySqlDbContextOptionsBuilder> MySqlOptionsAction  { get; set; }
        
        public void SetMySqlOptions(Action<MySqlDbContextOptionsBuilder> action)
        {
            MySqlOptionsAction = action;
        }
    }
}