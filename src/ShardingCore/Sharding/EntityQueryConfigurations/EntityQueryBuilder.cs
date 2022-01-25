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
        /// 使用当前属性order和comparer有关联
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="primaryOrderPropertyExpression"></param>
        /// <param name="isAsc">true:当前属性正序和comparer正序一样,false:当前属性倒序和comparer正序一样</param>
        /// <returns></returns>
        public EntityQueryBuilder<TEntity> AddOrder<TProperty>(Expression<Func<TEntity, TProperty>> primaryOrderPropertyExpression,bool isAsc=true)
        {
            _entityQueryMetadata.AddSeqComparerOrder(primaryOrderPropertyExpression.GetPropertyAccess().Name, isAsc);
            return this;
        }
        /// <summary>
        /// 添加链接限制,和程序启动配置的MaxQueryConnectionsLimit取最小值,非迭代器有效,说人话就是ToList不生效这个链接数限制
        /// </summary>
        /// <param name="connectionsLimit">连接数</param>
        /// <param name="methodNames">查询方法</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
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
        /// <summary>
        /// 配置默认方法不带排序的时候采用什么排序来触发熔断
        /// </summary>
        /// <param name="asc"></param>
        /// <param name="methodNames"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public EntityQueryBuilder<TEntity> AddDefaultSequenceQueryTrip(bool asc,params QueryableMethodNameEnum[] methodNames)
        {
            
            foreach (var methodName in methodNames)
            {
                _entityQueryMetadata.AddDefaultSequenceQueryTrip(asc,methodName);
            }
            return this;
        }
    }
}
