

namespace ShardingCore.DynamicDataSources
{
    public interface IDataSourceInitializer
    {
        /// <summary>
        /// 动态初始化数据源仅创建
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="createDatabase"></param>
        /// <param name="createTable"></param>
        void InitConfigure( string dataSourceName,bool createDatabase,bool createTable);
    }
}
