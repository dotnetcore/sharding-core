using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ShardingQueryExecutors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/31 21:30:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class EnumeratorShardingQueryExecutor<TEntity>:IShardingQueryExecutor<TEntity>
    {
        public MethodCallExpression GetQueryExpression()
        {
            throw new NotImplementedException();
        }

        public IShardingDbContext GetCurrentShardingDbContext()
        {
            throw new NotImplementedException();
        }
    }
}
