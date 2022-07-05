namespace ShardingCore
{
    public class InitConfigureUnit
    {
        public InitConfigureUnit(string dataSourceName)
        {
            DataSourceName = dataSourceName;
        }

        public string DataSourceName { get; }
    }
}
