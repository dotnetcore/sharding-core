using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/2 17:25:33
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal abstract class AbstractBaseMergeEngine
    {
        private readonly StreamMergeContext _streamMergeContext;

        public AbstractBaseMergeEngine(StreamMergeContext streamMergeContext)
        {
            _streamMergeContext = streamMergeContext;
        }

        protected StreamMergeContext GetStreamMergeContext()
        {
            return _streamMergeContext;
        }
        /// <summary>
        /// sql执行的路由最小单元
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ISqlRouteUnit> GetDefaultSqlRouteUnits()
        {
            if (_streamMergeContext.UseUnionAllMerge())
            {
                return _streamMergeContext.ShardingRouteResult.RouteUnits.GroupBy(o=>o.DataSourceName).Select(o=>new UnSupportSqlRouteUnit(o.Key,o.Select(g=>g.TableRouteResult).ToList()));
            }
            return _streamMergeContext.ShardingRouteResult.RouteUnits;
           
        }
    }
}
