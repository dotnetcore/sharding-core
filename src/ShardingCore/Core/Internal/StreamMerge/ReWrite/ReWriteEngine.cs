using System.Linq;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.StreamMerge.ReWrite
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 23:44:24
* @Email: 326308290@qq.com
*/
    internal class ReWriteEngine<T>
    {
        private readonly IQueryable<T> _queryable;

        public ReWriteEngine(IQueryable<T> queryable)
        {
            _queryable = queryable;
        }

        public ReWriteResult<T> ReWrite()
        {
            var extraEntry = _queryable.GetExtraEntry();
            var skip = extraEntry.Skip;
            var take = extraEntry.Take;
            var orders = extraEntry.Orders ?? Enumerable.Empty<PropertyOrder>();
            
            //去除分页,获取前Take+Skip数量
            var noPageSource = _queryable.RemoveTake().RemoveSkip();
            if (take.HasValue)
                noPageSource = noPageSource.Take(take.Value + skip.GetValueOrDefault());
            return new ReWriteResult<T>(_queryable,noPageSource,skip,take,orders);
        }
    }
}