using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Visitors.Querys
{
    public class CompileParseResult
    {
        public CompileParseResult(bool isUnion, bool? isNoTracking, bool isIgnoreFilter, ISet<Type> queryEntities)
        {
            IsUnion = isUnion;
            IsNoTracking = isNoTracking;
            IsIgnoreFilter = isIgnoreFilter;
            QueryEntities = queryEntities;
        }
        /// <summary>
        /// 是否使用了union查询
        /// </summary>
        public bool IsUnion { get; }
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
        public ISet<Type> QueryEntities { get; }
    }
}
