using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Visitors.Querys
{
    public class CompileParseResult
    {
        public CompileParseResult(bool? isNoTracking, bool isIgnoreFilter, Dictionary<Type, Expression> queryEntities)
        {
            IsNoTracking = isNoTracking;
            IsIgnoreFilter = isIgnoreFilter;
            QueryEntities = queryEntities;
        }
        /// <summary>
        /// 是否使用了追踪
        /// </summary>
        public bool? IsNoTracking { get; }
        /// <summary>
        /// 是否使用了忽略filter
        /// </summary>
        public bool IsIgnoreFilter { get; }
        /// <summary>
        /// 当前涉及到的查询对象
        /// </summary>
        public Dictionary<Type, Expression> QueryEntities { get; }
    }
}
