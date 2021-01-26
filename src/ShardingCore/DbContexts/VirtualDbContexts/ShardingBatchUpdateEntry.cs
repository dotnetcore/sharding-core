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
        public ShardingBatchUpdateEntry(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp, List<DbContext> dbContexts)
        {
            Where = @where;
            UpdateExp = updateExp;
            DbContexts = dbContexts;
        }

        public Expression<Func<T, bool>> Where {get;}
       public Expression<Func<T, T>> UpdateExp{get;}
       public List<DbContext> DbContexts { get; }
    }
}