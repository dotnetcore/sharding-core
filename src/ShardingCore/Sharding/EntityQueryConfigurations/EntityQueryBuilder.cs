using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Exceptions;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Internal;
#endif

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    public class EntityQueryBuilder<TEntity> where TEntity : class
    {
        private readonly EntityQueryMetadata _entityQueryMetadata;

        public EntityQueryBuilder(EntityQueryMetadata entityQueryMetadata)
        {
            _entityQueryMetadata = entityQueryMetadata;
        }
        /// <summary>
        /// 添加分表后缀排序
        /// </summary>
        /// <param name="tailComparer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public EntityQueryBuilder<TEntity> ShardingTailComparer(IComparer<string> tailComparer)
        {
            _entityQueryMetadata.DefaultTailComparer = tailComparer ?? throw new ArgumentNullException(nameof(tailComparer));
            return this;
        }
        /// <summary>
        /// 使用当前属性order和comparer一样
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="primaryOrderPropertyExpression"></param>
        /// <returns></returns>
        public EntityQueryBuilder<TEntity> AddOrder<TProperty>(Expression<Func<TEntity, TProperty>> primaryOrderPropertyExpression)
        {
            _entityQueryMetadata.SeqQueryOrders.Add(primaryOrderPropertyExpression.GetPropertyAccess().Name);
            return this;
        }
        public EntityQueryBuilder<TEntity> AddConnectionsLimit(int connectionsLimit,params QueryableMethodNameEnum[] methodNames)
        {
            if (connectionsLimit < 1)
                throw new ArgumentNullException($"{nameof(connectionsLimit)} should >= 1");
            foreach (var methodName in methodNames)
            {
                _entityQueryMetadata.AddConnectionsLimit(connectionsLimit,methodName);
            }
            return this;
        }
    }
}
