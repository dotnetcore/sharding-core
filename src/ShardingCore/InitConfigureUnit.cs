namespace ShardingCore
{
    /// <summary>
    /// 初始化配置执行单元
    /// </summary>
    public class InitConfigureUnit
    {
        /// <summary>
        /// 初始化配置执行单元
        /// </summary>
        public InitConfigureUnit(string dataSourceName)
        {
            DataSourceName = dataSourceName;
        }

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSourceName { get; }
    }
}
