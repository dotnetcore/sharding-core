using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Sharding.PaginationConfigurations;

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

            return paginationMetadata.ReverseFactor * total < skip;
        }
    }
}
