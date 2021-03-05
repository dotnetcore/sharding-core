using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.VirtualDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 01 January 2021 20:53:07
* @Email: 326308290@qq.com
*/
    public class ShardingBatchDeleteEntry<T>where T:class
    {
        public ShardingBatchDeleteEntry(Expression<Func<T, bool>> @where, List<(string connectKey,List<DbContext>)> dbContextGroups)
        {
            Where = @where;
            DbContextGroups = dbContextGroups;
        }

        public Expression<Func<T, bool>> Where{ get; }
        public List<(string connectKey, List<DbContext>)> DbContextGroups { get; }
    }
}