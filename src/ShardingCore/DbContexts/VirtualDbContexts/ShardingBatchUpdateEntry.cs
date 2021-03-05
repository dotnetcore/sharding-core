using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.VirtualDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 01 January 2021 20:45:16
* @Email: 326308290@qq.com
*/
    public class ShardingBatchUpdateEntry<T> where T:class
    {
        public ShardingBatchUpdateEntry(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp, List<(string connectKey, List<DbContext> dbContexts)> dbContextGroups)
        {
            Where = @where;
            UpdateExp = updateExp;
            DbContextGroups = dbContextGroups;
        }

        public Expression<Func<T, bool>> Where {get;}
       public Expression<Func<T, T>> UpdateExp{get;}
       public List<(string connectKey, List<DbContext> dbContexts)>  DbContextGroups { get; }
    }
}