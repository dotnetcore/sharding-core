using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeContexts
{
    public sealed class ParseResult:IParseResult
    {
        private readonly PaginationContext _paginationContext;
        private readonly OrderByContext _orderByContext;
        private readonly SelectContext _selectContext;
        private readonly GroupByContext _groupByContext;
        private readonly bool _isEnumerableQuery;
        private readonly string _queryMethodName;

        public ParseResult(PaginationContext paginationContext,
            OrderByContext orderByContext, 
            SelectContext selectContext,
            GroupByContext groupByContext,
            bool isEnumerableQuery,
            string queryMethodName)
        {
            _paginationContext = paginationContext;
            _orderByContext = orderByContext;
            _selectContext = selectContext;
            _groupByContext = groupByContext;
            _isEnumerableQuery = isEnumerableQuery;
            _queryMethodName = queryMethodName;
        }

        public PaginationContext GetPaginationContext()
        {
            return _paginationContext;
        }

        public OrderByContext GetOrderByContext()
        {
            return _orderByContext;
        }


        public SelectContext GetSelectContext()
        {
            return _selectContext;
        }

        public GroupByContext GetGroupByContext()
        {
            return _groupByContext;
        }

        public bool IsEmunerableQuery()
        {
            return _isEnumerableQuery;
        }

        public string QueryMethodName()
        {
            return _queryMethodName;
        }
    }
}
