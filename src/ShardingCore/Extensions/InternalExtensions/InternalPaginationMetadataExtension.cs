using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Extensions.InternalExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 13:21:09
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal  static class InternalPaginationMetadataExtension
    {
        internal static bool IsUseReverse(this PaginationMetadata paginationMetadata,int skip,long total)
        {
            if (total < paginationMetadata.ReverseTotalGe)
                return false;

            return skip> paginationMetadata.ReverseFactor * total;
        }
        internal static bool IsUseUneven(this PaginationMetadata paginationMetadata,ICollection<RouteQueryResult<long>> routeQueryResults,int skip)
        {
            if (routeQueryResults.Count <= 1)
                return false;

            if (skip < paginationMetadata.UnevenLimit)
                return false;
            var total = routeQueryResults.Sum(o => o.QueryResult);
            if(total* paginationMetadata.UnevenFactorGe < routeQueryResults.First().QueryResult)
                return false;
            return true;
        }
    }
}
