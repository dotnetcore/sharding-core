using System;

namespace ShardingCore.Jobs.Abstaractions
{
    internal interface IJob
    {
        string JobName { get; }
        bool StartJob();
    }
}
