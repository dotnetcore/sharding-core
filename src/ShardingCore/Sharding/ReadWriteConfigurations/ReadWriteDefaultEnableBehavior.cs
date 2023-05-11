namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /**
     * 读写分离默认行为
     */
    public enum ReadWriteDefaultEnableBehavior
    {
        //默认不启用
        DefaultDisable,
        //默认启用
        DefaultEnable,
        //不在事务中启用
        OutTransactionEnable
    }
}