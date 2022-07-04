namespace ShardingCore.Helpers
{

    public class MigrationResult
    {
        public MigrationCommandTypeEnum CommandType { get; set; } = MigrationCommandTypeEnum.OtherCommand;
        public bool InDataSource { get; set; } = true;
    }
}