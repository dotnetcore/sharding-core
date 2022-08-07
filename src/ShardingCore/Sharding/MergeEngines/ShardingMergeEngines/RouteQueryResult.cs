using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/1 13:13:17
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IRouteQueryResult
    {
        bool HasQueryResult();
    }
    public class RouteQueryResult<TResult>: IRouteQueryResult
    {
        private readonly bool _hasValue;
        public string DataSourceName { get; }
        public TableRouteResult TableRouteResult { get; }
        public TResult QueryResult { get; }

        public RouteQueryResult(string dataSourceName,TableRouteResult tableRouteResult,TResult queryResult)
        {
            DataSourceName = dataSourceName;
            TableRouteResult = tableRouteResult;
            QueryResult = queryResult;
            _hasValue = QueryResult != null;
        }
        public RouteQueryResult(string dataSourceName,TableRouteResult tableRouteResult,TResult queryResult,bool hasValue)
        {
            _hasValue = hasValue;
            DataSourceName = dataSourceName;
            TableRouteResult = tableRouteResult;
            QueryResult = queryResult;
        }

        public bool HasQueryResult()
        {
            return _hasValue;
        }
    }
}
