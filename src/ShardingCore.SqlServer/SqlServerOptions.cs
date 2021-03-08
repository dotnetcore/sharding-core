using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.SqlServer
{
/*
* @Author: xjm
* @Description:
* @Date: 2020年4月7日 8:34:04
* @Email: 326308290@qq.com
*/
    public class SqlServerOptions: AbstractShardingCoreOptions
    {
    }
}