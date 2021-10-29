namespace ShardingCore.Jobs.Abstaractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 08 January 2021 22:19:57
    * @Email: 326308290@qq.com
    */
    internal interface IJobExecutor
    {
        void Run();
        bool IsRunning();
        bool StartJob();
        void Complete();
    }
}