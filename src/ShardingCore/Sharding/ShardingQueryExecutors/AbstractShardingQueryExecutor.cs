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
    * @Date: 2021/8/30 17:11:40
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class AbstractShardingQueryExecutor<TEntity>: IShardingQueryExecutor<TEntity>
    {
        private readonly MethodCallExpression _expression;
        private readonly IShardingDbContext _shardingDbContext;

        public AbstractShardingQueryExecutor(MethodCallExpression expression,IShardingDbContext shardingDbContext)
        {
            _expression = expression;
            _shardingDbContext = shardingDbContext;
        }
        public MethodCallExpression GetQueryExpression()
        {
            return _expression;
        }

        public IShardingDbContext GetCurrentShardingDbContext()
        {
            return _shardingDbContext;
        }
    }
}
