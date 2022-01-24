using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    /// <summary>
    /// 对象顺序查询
    /// </summary>
    public class EntitySeqQueryConfig
    {
        public IComparer<string> RouteComparer { get; }
        public PropertyInfo PrimaryOrderPropertyInfo { get; }
        public int ParallelThreadQueryCount { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryOrderPropertyExpression">排序字段</param>
        /// <param name="parallelThreadQueryCount">并发线程数</param>
        /// <param name="routeComparer">tail后缀比较器 asc</param>
        /// <param name="maxTake">最大查询条数</param>
        public EntitySeqQueryConfig(LambdaExpression primaryOrderPropertyExpression,int parallelThreadQueryCount, IComparer<string> routeComparer = null)
        {
            if (primaryOrderPropertyExpression == null) throw new ArgumentNullException(nameof(primaryOrderPropertyExpression));
            if(parallelThreadQueryCount<=0) throw new ArgumentException(nameof(parallelThreadQueryCount));
            PrimaryOrderPropertyInfo = primaryOrderPropertyExpression.GetPropertyAccess();
            ParallelThreadQueryCount = parallelThreadQueryCount;
            RouteComparer=routeComparer??Comparer<string>.Default;
        }
    }
}
