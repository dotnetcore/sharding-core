using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.StreamMerge.GenericMerges;
using ShardingCore.Core.Internal.StreamMerge.ListMerge;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Extensions;
#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Core.Internal.StreamMerge.ListSourceMerges
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 14:45:19
* @Email: 326308290@qq.com
*/
    internal class StreamMergeListSourceEngine<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;

        public StreamMergeListSourceEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
        }
        
        public async Task<List<T>> Execute()
        {
             using (var engine =new GenericStreamMergeEngine<T>(_mergeContext))
             {
                 var enumerators = await engine.GetStreamEnumerator();
                 
                 return await new StreamMergeListEngine<T>(_mergeContext, enumerators).Execute();
             }
        }
    }
}